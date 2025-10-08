using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Handler for retry failed payment command
    /// </summary>
    public class RetryFailedPaymentCommandHandler : IRequestHandler<RetryFailedPaymentCommand, RetryFailedPaymentResult>
    {
        private readonly IPaymentRetryService _retryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<RetryFailedPaymentCommandHandler> _logger;

        public RetryFailedPaymentCommandHandler(
            IPaymentRetryService retryService,
            IUnitOfWork unitOfWork,
            IUserContextService userContextService,
            ILogger<RetryFailedPaymentCommandHandler> logger)
        {
            _retryService = retryService;
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<RetryFailedPaymentResult> Handle(RetryFailedPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing retry request for payment {PaymentId} by client {ClientId}", 
                    request.PaymentId, request.ClientId);

                // Get current user context
                var currentClientId = await _userContextService.GetCurrentClientIdAsync();
                if (currentClientId != request.ClientId)
                {
                    _logger.LogWarning("Client {ClientId} attempted to retry payment {PaymentId} belonging to different client", 
                        currentClientId, request.PaymentId);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = "You can only retry your own payments"
                    };
                }

                // Rate limiting: Check if client has exceeded payment retry limits
                var rateLimitDelay = await _unitOfWork.Payments.GetRateLimitDelayAsync(request.ClientId, cancellationToken);
                if (rateLimitDelay.HasValue)
                {
                    _logger.LogWarning("Client {ClientId} rate limited for payment retries. Retry after {Minutes} minutes",
                        request.ClientId, rateLimitDelay.Value.TotalMinutes);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"Rate limit exceeded. Please try again in {Math.Ceiling(rateLimitDelay.Value.TotalMinutes)} minutes"
                    };
                }

                // SECURITY: Get the payment with client verification
                var payment = await _unitOfWork.Payments.GetByIdForClientAsync(request.PaymentId, request.ClientId, cancellationToken);
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found or does not belong to client {ClientId}", request.PaymentId, request.ClientId);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = "Payment not found"
                    };
                }

                // Check if payment is in failed status
                if (payment.Status != PaymentStatus.Failed)
                {
                    _logger.LogWarning("Payment {PaymentId} is not in failed status, current status: {Status}", 
                        request.PaymentId, payment.Status);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = "Only failed payments can be retried"
                    };
                }

                // Check if there's already an active retry
                var hasActiveRetry = await _retryService.HasActiveRetriesAsync(request.PaymentId, cancellationToken);
                if (hasActiveRetry)
                {
                    _logger.LogWarning("Payment {PaymentId} already has an active retry schedule", request.PaymentId);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = "Payment already has an active retry schedule"
                    };
                }

                // TRANSACTION: Ensure atomicity of retry scheduling and rate limit recording
                var transactionResult = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                if (!transactionResult.IsSuccess)
                {
                    _logger.LogError("Failed to begin transaction for payment retry: {Error}", transactionResult.ErrorMessage);
                    return new RetryFailedPaymentResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to begin transaction"
                    };
                }

                try
                {
                    // Calculate next attempt number using service method (domain logic)
                    var attemptNumber = await _retryService.CalculateNextAttemptNumberAsync(request.PaymentId, cancellationToken);

                    // FIXED: Schedule the retry FIRST, then record attempt (correct order)
                    var createdSchedule = await _retryService.ScheduleRetryAsync(
                        request.PaymentId,
                        attemptNumber,
                        request.Reason ?? "Manual retry requested",
                        cancellationToken);

                    // Only record payment attempt AFTER successful scheduling
                    await _unitOfWork.Payments.RecordPaymentAttemptAsync(request.ClientId, cancellationToken);

                    // Commit transaction - ensures both operations succeed or both fail
                    var commitResult = await _unitOfWork.CommitAsync(cancellationToken);
                    if (!commitResult.IsSuccess)
                    {
                        _logger.LogError("Failed to commit transaction for payment retry: {Error}", commitResult.ErrorMessage);
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        return new RetryFailedPaymentResult
                        {
                            Success = false,
                            ErrorMessage = "Failed to commit transaction"
                        };
                    }

                    _logger.LogInformation("Successfully scheduled retry {RetryId} for payment {PaymentId}",
                        createdSchedule.Id, request.PaymentId);

                    return new RetryFailedPaymentResult
                    {
                        Success = true,
                        RetryScheduleId = createdSchedule.Id,
                        NextAttemptAt = createdSchedule.NextAttemptAt,
                        AttemptNumber = createdSchedule.AttemptNumber,
                        MaxAttempts = createdSchedule.MaxAttempts
                    };
                }
                catch
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry request for payment {PaymentId}", request.PaymentId);
                return new RetryFailedPaymentResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while processing the retry request"
                };
            }
        }
    }
}
