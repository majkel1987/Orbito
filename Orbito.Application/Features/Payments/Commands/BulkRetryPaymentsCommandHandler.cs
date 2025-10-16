using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler for bulk retry payments command
    /// </summary>
    public class BulkRetryPaymentsCommandHandler : IRequestHandler<BulkRetryPaymentsCommand, BulkRetryPaymentsResult>
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
            _retryService = retryService;
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<BulkRetryPaymentsResult> Handle(BulkRetryPaymentsCommand request, CancellationToken cancellationToken)
        {
            var result = new BulkRetryPaymentsResult
            {
                TotalProcessed = request.PaymentIds.Count,
                Results = new List<BulkRetryItemResult>()
            };

            try
            {
                _logger.LogInformation("Processing bulk retry request for {Count} payments by client {ClientId}",
                    request.PaymentIds.Count, request.ClientId);

                // Rate limiting check
                if (request.PaymentIds.Count > MaxBulkRetryLimit)
                {
                    _logger.LogWarning("Client {ClientId} attempted bulk retry with {Count} payments, exceeding limit of {Limit}",
                        request.ClientId, request.PaymentIds.Count, MaxBulkRetryLimit);
                    return new BulkRetryPaymentsResult
                    {
                        TotalProcessed = 0,
                        SuccessfulRetries = 0,
                        FailedRetries = 1,
                        Results = new List<BulkRetryItemResult>
                        {
                            new BulkRetryItemResult
                            {
                                PaymentId = Guid.Empty,
                                Success = false,
                                ErrorMessage = $"Maximum {MaxBulkRetryLimit} payments allowed per bulk retry operation"
                            }
                        }
                    };
                }

                // Get current user context
                var currentClientId = await _userContextService.GetCurrentClientIdAsync();
                if (currentClientId != request.ClientId)
                {
                    _logger.LogWarning("Client {ClientId} attempted bulk retry for different client {RequestClientId}",
                        currentClientId, request.ClientId);
                    return new BulkRetryPaymentsResult
                    {
                        TotalProcessed = 0,
                        SuccessfulRetries = 0,
                        FailedRetries = 1,
                        Results = new List<BulkRetryItemResult>
                        {
                            new BulkRetryItemResult
                            {
                                PaymentId = Guid.Empty,
                                Success = false,
                                ErrorMessage = "You can only retry your own payments"
                            }
                        }
                    };
                }

                // Rate limiting: Check if client has exceeded payment retry limits
                var rateLimitDelay = await _unitOfWork.Payments.GetRateLimitDelayAsync(request.ClientId, cancellationToken);
                if (rateLimitDelay.HasValue)
                {
                    _logger.LogWarning("Client {ClientId} rate limited for bulk payment retries. Retry after {Minutes} minutes",
                        request.ClientId, rateLimitDelay.Value.TotalMinutes);
                    return new BulkRetryPaymentsResult
                    {
                        TotalProcessed = 0,
                        SuccessfulRetries = 0,
                        FailedRetries = 1,
                        Results = new List<BulkRetryItemResult>
                        {
                            new BulkRetryItemResult
                            {
                                PaymentId = Guid.Empty,
                                Success = false,
                                ErrorMessage = $"Rate limit exceeded. Please try again in {Math.Ceiling(rateLimitDelay.Value.TotalMinutes)} minutes"
                            }
                        }
                    };
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
                    return CreateErrorResult(request.PaymentIds, "Failed to begin transaction");
                }

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

                        result.Results.Add(itemResult);

                        if (itemResult.Success)
                        {
                            result.SuccessfulRetries++;
                        }
                        else
                        {
                            result.FailedRetries++;
                        }
                    }

                    // Record payment attempts AFTER all schedules are created successfully
                    // OPTIMIZATION: Use batch method instead of loop to reduce database round-trips
                    if (result.SuccessfulRetries > 0)
                    {
                        await _unitOfWork.Payments.RecordPaymentAttemptsAsync(
                            request.ClientId,
                            result.SuccessfulRetries,
                            cancellationToken);
                    }

                    // COMMIT TRANSACTION
                    var commitResult = await _unitOfWork.CommitAsync(cancellationToken);
                    if (!commitResult.IsSuccess)
                    {
                        _logger.LogError("Failed to commit transaction: {Error}", commitResult.ErrorMessage);
                        return CreateErrorResult(request.PaymentIds, "Failed to commit transaction");
                    }

                    _logger.LogInformation("Bulk retry completed: {Successful} successful, {Failed} failed out of {Total}",
                        result.SuccessfulRetries, result.FailedRetries, result.TotalProcessed);

                    return result;
                }
                catch (Exception)
                {
                    // ROLLBACK on any error
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk retry request for {Count} payments. Processed {Successful} successfully before error.",
                    request.PaymentIds.Count, result.SuccessfulRetries);

                // Return accurate information about what was processed
                var remainingIds = request.PaymentIds.Skip(result.Results.Count).ToList();
                foreach (var id in remainingIds)
                {
                    result.Results.Add(new BulkRetryItemResult
                    {
                        PaymentId = id,
                        Success = false,
                        ErrorMessage = "Not processed due to earlier error"
                    });
                }

                result.FailedRetries = request.PaymentIds.Count - result.SuccessfulRetries;
                return result;
            }
        }

        private static BulkRetryPaymentsResult CreateErrorResult(List<Guid> paymentIds, string errorMessage)
        {
            return new BulkRetryPaymentsResult
            {
                TotalProcessed = paymentIds.Count,
                SuccessfulRetries = 0,
                FailedRetries = paymentIds.Count,
                Results = paymentIds.Select(id => new BulkRetryItemResult
                {
                    PaymentId = id,
                    Success = false,
                    ErrorMessage = errorMessage
                }).ToList()
            };
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
                        ErrorMessage = "Payment not found or does not belong to client"
                    };
                }

                // Check if payment is in failed status
                if (payment.Status != PaymentStatus.Failed)
                {
                    return new BulkRetryItemResult
                    {
                        PaymentId = paymentId,
                        Success = false,
                        ErrorMessage = "Only failed payments can be retried"
                    };
                }

                // Check if there's already an active retry (from pre-loaded data)
                if (activeRetriesDict.ContainsKey(paymentId))
                {
                    return new BulkRetryItemResult
                    {
                        PaymentId = paymentId,
                        Success = false,
                        ErrorMessage = "Payment already has an active retry schedule"
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
}
