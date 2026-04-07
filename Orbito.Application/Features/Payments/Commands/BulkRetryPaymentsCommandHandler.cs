using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Handler for bulk retry payments command
/// </summary>
public class BulkRetryPaymentsCommandHandler : IRequestHandler<BulkRetryPaymentsCommand, Result<BulkRetryPaymentsResponse>>
{
        private readonly IPaymentRetryService _retryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<BulkRetryPaymentsCommandHandler> _logger;

        // Rate limiting: max 50 payments per bulk operation
        private const int MaxBulkRetryLimit = 50;

        public BulkRetryPaymentsCommandHandler(
            IPaymentRetryService retryService,
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            ILogger<BulkRetryPaymentsCommandHandler> logger)
        {
            _retryService = retryService ?? throw new ArgumentNullException(nameof(retryService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<BulkRetryPaymentsResponse>> Handle(BulkRetryPaymentsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing bulk retry request for {Count} payments by client {ClientId}",
                    request.PaymentIds.Count, request.ClientId);

                // Rate limiting check
                if (request.PaymentIds.Count > MaxBulkRetryLimit)
                {
                    _logger.LogWarning("Client {ClientId} attempted bulk retry with {Count} payments, exceeding limit of {Limit}",
                        request.ClientId, request.PaymentIds.Count, MaxBulkRetryLimit);
                    return Result.Failure<BulkRetryPaymentsResponse>(
                        DomainErrors.Validation.OutOfRange("PaymentIds", 1, MaxBulkRetryLimit));
                }

                // Get current user context
                var currentClientId = await _userContextService.GetCurrentClientIdAsync();
                if (currentClientId != request.ClientId)
                {
                    _logger.LogWarning("Client {ClientId} attempted bulk retry for different client {RequestClientId}",
                        currentClientId, request.ClientId);
                    return Result.Failure<BulkRetryPaymentsResponse>(DomainErrors.General.Unauthorized);
                }

                // Rate limiting: Check if client has exceeded payment retry limits
                var rateLimitDelay = await _unitOfWork.Payments.GetRateLimitDelayAsync(request.ClientId, cancellationToken);
                if (rateLimitDelay.HasValue)
                {
                    _logger.LogWarning("Client {ClientId} rate limited for bulk payment retries. Retry after {Minutes} minutes",
                        request.ClientId, rateLimitDelay.Value.TotalMinutes);
                    return Result.Failure<BulkRetryPaymentsResponse>(DomainErrors.Payment.RateLimitExceeded);
                }

                // BATCH OPERATION: Load all payments at once to avoid N+1
                var paymentsDict = await _unitOfWork.Payments.GetByIdsForClientAsync(request.PaymentIds, request.ClientId, cancellationToken);

                // BATCH OPERATION: Load all active retries at once (with client security filtering)
                var activeRetriesDict = await _retryService.GetActiveRetriesForPaymentsAsync(request.PaymentIds, request.ClientId, cancellationToken);

                // BEGIN TRANSACTION for atomicity
                var transactionResult = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                if (!transactionResult.IsSuccess)
                {
                    _logger.LogError("Failed to begin transaction: {Error}", transactionResult.ErrorMessage);
                    return Result.Failure<BulkRetryPaymentsResponse>(DomainErrors.General.UnexpectedError);
                }

                var results = new List<BulkRetryItemResult>();
                int successfulRetries = 0;
                int failedRetries = 0;

                try
                {
                    // Process each payment individually within transaction
                    foreach (var paymentId in request.PaymentIds)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var itemResult = await ProcessSingleRetryAsync(
                            paymentId,
                            request.ClientId,
                            request.Reason,
                            paymentsDict,
                            activeRetriesDict,
                            cancellationToken);

                        results.Add(itemResult);

                        if (itemResult.Success)
                        {
                            successfulRetries++;
                        }
                        else
                        {
                            failedRetries++;
                        }
                    }

                    // Record payment attempts AFTER all schedules are created successfully
                    // OPTIMIZATION: Use batch method instead of loop to reduce database round-trips
                    if (successfulRetries > 0)
                    {
                        await _unitOfWork.Payments.RecordPaymentAttemptsAsync(
                            request.ClientId,
                            successfulRetries,
                            cancellationToken);
                    }

                    // COMMIT TRANSACTION
                    var commitResult = await _unitOfWork.CommitAsync(cancellationToken);
                    if (!commitResult.IsSuccess)
                    {
                        _logger.LogError("Failed to commit transaction: {Error}", commitResult.ErrorMessage);
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        return Result.Failure<BulkRetryPaymentsResponse>(DomainErrors.General.UnexpectedError);
                    }

                    var response = new BulkRetryPaymentsResponse
                    {
                        TotalProcessed = request.PaymentIds.Count,
                        SuccessfulRetries = successfulRetries,
                        FailedRetries = failedRetries,
                        Results = results
                    };

                    _logger.LogInformation("Bulk retry completed: {Successful} successful, {Failed} failed out of {Total}",
                        response.SuccessfulRetries, response.FailedRetries, response.TotalProcessed);

                    return Result.Success(response);
                }
                catch (Exception)
                {
                    // ROLLBACK on any error
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                // Rethrow cancellation exceptions - they should not be caught
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk retry request for {Count} payments",
                    request.PaymentIds.Count);
                return Result.Failure<BulkRetryPaymentsResponse>(DomainErrors.General.UnexpectedError);
            }
        }

        /// <summary>
        /// Processes a single payment retry using pre-loaded data (batch optimization)
        /// </summary>
        private async Task<BulkRetryItemResult> ProcessSingleRetryAsync(
            Guid paymentId,
            Guid clientId,
            string? reason,
            Dictionary<Guid, Payment> paymentsDict,
            Dictionary<Guid, PaymentRetrySchedule> activeRetriesDict,
            CancellationToken cancellationToken)
        {
            try
            {
                // SECURITY: Get the payment from pre-loaded dictionary (already filtered by client)
                if (!paymentsDict.TryGetValue(paymentId, out var payment))
                {
                    return new BulkRetryItemResult
                    {
                        PaymentId = paymentId,
                        Success = false,
                        ErrorMessage = DomainErrors.Payment.NotFound.Message
                    };
                }

                // Check if payment is in failed status
                if (payment.Status != PaymentStatus.Failed)
                {
                    return new BulkRetryItemResult
                    {
                        PaymentId = paymentId,
                        Success = false,
                        ErrorMessage = DomainErrors.PaymentRetry.NotFailedStatus.Message
                    };
                }

                // Check if there's already an active retry (from pre-loaded data)
                if (activeRetriesDict.ContainsKey(paymentId))
                {
                    return new BulkRetryItemResult
                    {
                        PaymentId = paymentId,
                        Success = false,
                        ErrorMessage = DomainErrors.PaymentRetry.AlreadyActive.Message
                    };
                }

                // Calculate next attempt number using service method (domain logic)
                var attemptNumber = await _retryService.CalculateNextAttemptNumberAsync(paymentId, cancellationToken);

                // Schedule the retry - now returns the full object
                var createdSchedule = await _retryService.ScheduleRetryAsync(
                    paymentId,
                    clientId,
                    attemptNumber,
                    reason ?? "Bulk retry requested",
                    cancellationToken);

                return new BulkRetryItemResult
                {
                    PaymentId = paymentId,
                    Success = true,
                    RetryScheduleId = createdSchedule.Id,
                    NextAttemptAt = createdSchedule.NextAttemptAt,
                    AttemptNumber = createdSchedule.AttemptNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry for payment {PaymentId}", paymentId);
                return new BulkRetryItemResult
                {
                    PaymentId = paymentId,
                    Success = false,
                    ErrorMessage = $"An error occurred while processing the retry: {ex.Message}"
                };
            }
        }
    }
