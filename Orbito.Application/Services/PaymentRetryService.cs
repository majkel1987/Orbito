using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Options;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Services
{
    /// <summary>
    /// Service for managing payment retry logic with exponential backoff
    /// </summary>
    public class PaymentRetryService : IPaymentRetryService
    {
        private readonly IPaymentRetryRepository _retryRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<PaymentRetryService> _logger;
        private readonly PaymentRetryOptions _options;

        public PaymentRetryService(
            IPaymentRetryRepository retryRepository,
            IPaymentRepository paymentRepository,
            ITenantProvider tenantProvider,
            ILogger<PaymentRetryService> logger,
            IOptions<PaymentRetryOptions> options)
        {
            _retryRepository = retryRepository;
            _paymentRepository = paymentRepository;
            _tenantProvider = tenantProvider;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Schedules a retry for a failed payment
        /// NOTE: Race condition protection is handled by unique constraint in database:
        /// IX_PaymentRetrySchedule_Payment_Active on PaymentId WHERE Status IN ('Scheduled', 'InProgress')
        /// </summary>
        public async Task<PaymentRetrySchedule> ScheduleRetryAsync(Guid paymentId, Guid clientId, int attemptNumber, string errorReason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Scheduling retry for payment {PaymentId}, attempt {AttemptNumber}", paymentId, attemptNumber);

            // Validate attempt number
            if (!PaymentRetrySchedule.ValidateAttemptNumber(attemptNumber))
            {
                throw new ArgumentException($"Invalid attempt number: {attemptNumber}. Must be between 1 and {_options.MaxAttempts}.");
            }

            // Use secure method with client verification
            var payment = await _paymentRepository.GetByIdForClientAsync(paymentId, clientId, cancellationToken);
            if (payment == null)
            {
                throw new InvalidOperationException($"Payment {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Failed)
            {
                throw new InvalidOperationException($"Payment {paymentId} is not in failed status");
            }

            // Check if there's already an active retry for this payment
            var existingRetry = await _retryRepository.GetActiveRetryByPaymentIdAsync(paymentId, cancellationToken);
            if (existingRetry != null)
            {
                throw new InvalidOperationException($"Payment {paymentId} already has an active retry schedule");
            }

            // Create new retry schedule
            var tenantId = _tenantProvider.GetCurrentTenantId();
            if (tenantId == null)
            {
                throw new InvalidOperationException("Tenant context is required for creating retry schedule");
            }

            var retrySchedule = PaymentRetrySchedule.Create(
                tenantId,
                payment.ClientId,
                paymentId,
                attemptNumber,
                maxAttempts: _options.MaxAttempts,
                lastError: errorReason);

            try
            {
                await _retryRepository.AddAsync(retrySchedule, cancellationToken);
                await _retryRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Retry scheduled for payment {PaymentId} with ID {RetryId}, next attempt at {NextAttemptAt}",
                    paymentId, retrySchedule.Id, retrySchedule.NextAttemptAt);

                return retrySchedule;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_PaymentRetrySchedule_Payment_Active") == true)
            {
                // Duplicate active retry detected by unique constraint
                _logger.LogWarning("Duplicate retry schedule attempt for payment {PaymentId} prevented by database constraint", paymentId);
                throw new InvalidOperationException($"Payment {paymentId} already has an active retry schedule", ex);
            }
        }

        /// <summary>
        /// Processes all scheduled retries that are due (including stuck InProgress retries)
        /// </summary>
        public async Task<int> ProcessScheduledRetriesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing scheduled retries");

            var dueRetries = await _retryRepository.GetDueRetriesAsync(
                DateTime.UtcNow,
                _options.InProgressTimeout,
                cancellationToken);

            if (dueRetries.Count == 0)
            {
                _logger.LogInformation("No retries due for processing");
                return 0;
            }

            _logger.LogInformation("Found {Count} retries due for processing", dueRetries.Count);

            // Parallel processing with concurrency limit
            var processedCount = 0;
            using var semaphore = new SemaphoreSlim(_options.MaxConcurrency);
            var tasks = dueRetries.Select(async retry =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    await ProcessRetryAsync(retry, cancellationToken);
                    Interlocked.Increment(ref processedCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing retry {RetryId} for payment {PaymentId}",
                        retry.Id, retry.PaymentId);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Processed {Count} scheduled retries", processedCount);
            return processedCount;
        }

        /// <summary>
        /// Calculates the next retry time using exponential backoff
        /// </summary>
        public DateTime CalculateNextRetryTime(int attemptNumber)
        {
            var delayIndex = Math.Min(attemptNumber - 1, _options.RetryDelays.Length - 1);
            return DateTime.UtcNow.Add(_options.RetryDelays[delayIndex]);
        }

        /// <summary>
        /// Cancels all scheduled retries for a payment
        /// </summary>
        public async Task<int> CancelScheduledRetriesAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cancelling retries for payment {PaymentId}", paymentId);

            var activeRetries = await _retryRepository.GetActiveRetriesByPaymentIdAsync(paymentId, cancellationToken);
            var cancelledCount = 0;

            foreach (var retry in activeRetries)
            {
                retry.MarkAsCancelled();
                cancelledCount++;
            }

            if (cancelledCount > 0)
            {
                await _retryRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Cancelled {Count} retries for payment {PaymentId}", cancelledCount, paymentId);
            }

            return cancelledCount;
        }

        /// <summary>
        /// Gets retry schedules for a specific payment
        /// </summary>
        public async Task<List<PaymentRetrySchedule>> GetRetrySchedulesAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            return await _retryRepository.GetRetrySchedulesByPaymentIdAsync(paymentId, cancellationToken);
        }

        /// <summary>
        /// Checks if a payment has any active retry schedules
        /// </summary>
        public async Task<bool> HasActiveRetriesAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var activeRetry = await _retryRepository.GetActiveRetryByPaymentIdAsync(paymentId, cancellationToken);
            return activeRetry != null;
        }

        /// <summary>
        /// Gets the next retry attempt for a payment
        /// </summary>
        public async Task<PaymentRetrySchedule?> GetNextRetryAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            return await _retryRepository.GetNextRetryByPaymentIdAsync(paymentId, cancellationToken);
        }

        /// <summary>
        /// Gets active retries for multiple payments (batch operation)
        /// </summary>
        public async Task<Dictionary<Guid, PaymentRetrySchedule>> GetActiveRetriesForPaymentsAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default)
        {
            // SECURITY: Pass clientId to prevent cross-client data leak
            var activeRetries = await _retryRepository.GetRetrySchedulesByPaymentIdsAsync(paymentIds, clientId, cancellationToken);

            // Group by PaymentId and take only active retries (Scheduled or InProgress)
            return activeRetries
                .Where(r => r.Status == RetryStatus.Scheduled || r.Status == RetryStatus.InProgress)
                .GroupBy(r => r.PaymentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.NextAttemptAt).First());
        }

        /// <summary>
        /// Calculates the next attempt number for a payment
        /// </summary>
        public async Task<int> CalculateNextAttemptNumberAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var nextRetry = await GetNextRetryAsync(paymentId, cancellationToken);
            var nextAttempt = nextRetry?.AttemptNumber + 1 ?? 1;
            return Math.Min(nextAttempt, _options.MaxAttempts);
        }

        /// <summary>
        /// Processes a single retry attempt
        /// </summary>
        private async Task ProcessRetryAsync(PaymentRetrySchedule retry, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing retry {RetryId} for payment {PaymentId}", retry.Id, retry.PaymentId);

            await using var transaction = await _retryRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                retry.MarkAsInProgress();
                await _retryRepository.SaveChangesAsync(cancellationToken);

                // Get payment...
                var payment = await _paymentRepository.GetByIdUnsafeAsync(retry.PaymentId, cancellationToken);
                
                if (payment == null)
                {
                    retry.MarkAsCancelled();
                    await _retryRepository.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                if (payment.Status != PaymentStatus.Failed)
                {
                    retry.MarkAsCancelled();
                    await _retryRepository.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                var retryResult = await AttemptPaymentRetryAsync(payment, cancellationToken);

                if (retryResult.Success)
                {
                    retry.MarkAsCompleted();
                }
                else
                {
                    if (retry.HasReachedMaxAttempts())
                    {
                        retry.MarkAsFailed(retryResult.ErrorMessage);
                    }
                    else
                    {
                        retry.UpdateForNextAttempt(retry.AttemptNumber + 1, retryResult.ErrorMessage);
                    }
                }

                await _retryRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry {RetryId}", retry.Id);
                
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogCritical(rollbackEx, "CRITICAL: Failed to rollback transaction for retry {RetryId}", retry.Id);
                }

                // Cleanup attempt
                try
                {
                    await using var cleanupTx = await _retryRepository.BeginTransactionAsync(cancellationToken);
                    retry.MarkAsCancelled();
                    await _retryRepository.SaveChangesAsync(cancellationToken);
                    await cleanupTx.CommitAsync(cancellationToken);
                }
                catch (Exception saveEx)
                {
                    _logger.LogCritical(saveEx, "CRITICAL: Failed to save cancelled status for retry {RetryId}. May be stuck in InProgress.", retry.Id);
                }

                throw;
            }
        }

        /// <summary>
        /// Attempts to retry a payment
        /// NOTE: This is a placeholder - actual payment retry integration not implemented yet
        /// </summary>
        private async Task<(bool Success, string? ErrorMessage)> AttemptPaymentRetryAsync(Payment payment, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to retry payment {PaymentId}", payment.Id);

            if (!_options.EnablePaymentProcessing)
            {
                _logger.LogWarning("Payment processing disabled (EnablePaymentProcessing=false). Simulating failure for dev/test.");
                await Task.Delay(100, cancellationToken);
                return (false, "Payment processing disabled in configuration (development/testing mode)");
            }

            // TODO: Implement actual payment retry logic with Stripe
            throw new NotImplementedException(
                "Payment retry integration not implemented yet. " +
                "Integrate with payment gateway (e.g., Stripe) to process the retry. " +
                "Set EnablePaymentProcessing=false in appsettings.json to simulate retries without actual payment processing.");
        }
    }
}
