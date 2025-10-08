using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    /// <summary>
    /// Entity for managing payment retry schedules with exponential backoff
    /// </summary>
    public class PaymentRetrySchedule : IMustHaveTenant
    {
        public const int DefaultMaxAttempts = 5;
        public const int MaxErrorMessageLength = 2000;
        private const int OverdueThresholdMinutes = 5;

        /// <summary>
        /// Unique identifier for the retry schedule
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public TenantId TenantId { get; private set; }

        /// <summary>
        /// Client ID for client-level data isolation
        /// </summary>
        public Guid ClientId { get; private set; }

        /// <summary>
        /// Navigation property to Client
        /// </summary>
        public Client Client { get; private set; } = default!;

        /// <summary>
        /// Payment ID that this retry schedule belongs to
        /// </summary>
        public Guid PaymentId { get; private set; }

        /// <summary>
        /// Navigation property to Payment
        /// </summary>
        public Payment Payment { get; private set; } = default!;

        /// <summary>
        /// When the next retry attempt should be made
        /// </summary>
        public DateTime NextAttemptAt { get; private set; }

        /// <summary>
        /// Current attempt number (1-based)
        /// </summary>
        public int AttemptNumber { get; private set; }

        /// <summary>
        /// Maximum number of retry attempts allowed
        /// </summary>
        public int MaxAttempts { get; private set; }

        /// <summary>
        /// Current status of the retry schedule
        /// </summary>
        public RetryStatus Status { get; private set; }

        /// <summary>
        /// Last error message from the failed attempt
        /// </summary>
        public string? LastError { get; private set; }

        /// <summary>
        /// When this retry schedule was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// When this retry schedule was last updated
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        private PaymentRetrySchedule() { } // EF Core

        /// <summary>
        /// Creates a new payment retry schedule
        /// </summary>
        public static PaymentRetrySchedule Create(
            TenantId tenantId,
            Guid clientId,
            Guid paymentId,
            int attemptNumber,
            int maxAttempts = DefaultMaxAttempts,
            string? lastError = null)
        {
            if (tenantId == null || tenantId.Value == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

            if (clientId == Guid.Empty)
                throw new ArgumentException("ClientId cannot be empty", nameof(clientId));

            if (paymentId == Guid.Empty)
                throw new ArgumentException("PaymentId cannot be empty", nameof(paymentId));

            if (attemptNumber < 1)
                throw new ArgumentException("AttemptNumber must be greater than 0", nameof(attemptNumber));

            if (maxAttempts < 1)
                throw new ArgumentException("MaxAttempts must be greater than 0", nameof(maxAttempts));

            if (attemptNumber > maxAttempts)
                throw new ArgumentException("AttemptNumber cannot exceed MaxAttempts", nameof(attemptNumber));

            var truncatedError = TruncateErrorMessage(lastError);

            var schedule = new PaymentRetrySchedule
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = clientId,
                PaymentId = paymentId,
                AttemptNumber = attemptNumber,
                MaxAttempts = maxAttempts,
                Status = RetryStatus.Scheduled,
                LastError = truncatedError,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            schedule.NextAttemptAt = schedule.CalculateNextRetryTime();
            return schedule;
        }

        /// <summary>
        /// Calculates the next retry time using exponential backoff
        /// </summary>
        /// <returns>DateTime for the next retry attempt</returns>
        public DateTime CalculateNextRetryTime()
        {
            // Exponential backoff: 5m, 15m, 1h, 6h, 24h
            var delays = new[]
            {
                TimeSpan.FromMinutes(5),   // 1st retry: 5 minutes
                TimeSpan.FromMinutes(15),  // 2nd retry: 15 minutes
                TimeSpan.FromHours(1),     // 3rd retry: 1 hour
                TimeSpan.FromHours(6),     // 4th retry: 6 hours
                TimeSpan.FromHours(24)     // 5th retry: 24 hours
            };

            var delayIndex = Math.Max(0, Math.Min(AttemptNumber - 1, delays.Length - 1));
            return DateTime.UtcNow.Add(delays[delayIndex]);
        }

        /// <summary>
        /// Validates if the attempt number is within valid range
        /// </summary>
        /// <param name="attemptNumber">Attempt number to validate</param>
        /// <param name="maxAttempts">Maximum allowed attempts</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateAttemptNumber(int attemptNumber, int maxAttempts = DefaultMaxAttempts)
        {
            return attemptNumber > 0 && attemptNumber <= maxAttempts;
        }

        /// <summary>
        /// Checks if this retry can be attempted
        /// </summary>
        /// <returns>True if retry is allowed, false otherwise</returns>
        public bool CanRetry()
        {
            var now = DateTime.UtcNow;
            return Status == RetryStatus.Scheduled &&
                   AttemptNumber <= MaxAttempts &&
                   NextAttemptAt <= now;
        }

        /// <summary>
        /// Marks the retry as in progress
        /// </summary>
        public void MarkAsInProgress()
        {
            if (Status != RetryStatus.Scheduled)
                throw new InvalidOperationException("Only scheduled retries can be marked as in progress");

            Status = RetryStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the retry as completed
        /// </summary>
        public void MarkAsCompleted()
        {
            if (Status != RetryStatus.InProgress)
                throw new InvalidOperationException("Only in-progress retries can be marked as completed");

            Status = RetryStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the retry as cancelled
        /// </summary>
        public void MarkAsCancelled()
        {
            if (Status == RetryStatus.Completed)
                throw new InvalidOperationException("Completed retries cannot be cancelled");

            Status = RetryStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancels the retry schedule (public method for external use)
        /// </summary>
        public void Cancel()
        {
            MarkAsCancelled();
        }

        /// <summary>
        /// Updates the retry with new attempt information
        /// </summary>
        /// <param name="attemptNumber">New attempt number</param>
        /// <param name="errorMessage">Error message from the failed attempt</param>
        public void UpdateForNextAttempt(int attemptNumber, string? errorMessage = null)
        {
            if (Status != RetryStatus.InProgress)
                throw new InvalidOperationException("Only in-progress retries can be updated");

            if (attemptNumber < 1)
                throw new ArgumentException("AttemptNumber must be greater than 0", nameof(attemptNumber));

            if (attemptNumber > MaxAttempts)
                throw new ArgumentException($"AttemptNumber cannot exceed MaxAttempts ({MaxAttempts})", nameof(attemptNumber));

            AttemptNumber = attemptNumber;
            LastError = TruncateErrorMessage(errorMessage);
            NextAttemptAt = CalculateNextRetryTime();
            Status = RetryStatus.Scheduled;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if maximum attempts have been reached
        /// </summary>
        /// <returns>True if max attempts reached, false otherwise</returns>
        public bool HasReachedMaxAttempts()
        {
            return AttemptNumber >= MaxAttempts;
        }

        /// <summary>
        /// Checks if the retry is overdue (should have been processed already)
        /// </summary>
        /// <returns>True if overdue, false otherwise</returns>
        public bool IsOverdue()
        {
            return Status == RetryStatus.Scheduled && NextAttemptAt < DateTime.UtcNow.AddMinutes(-OverdueThresholdMinutes);
        }

        /// <summary>
        /// Marks the retry as failed after exhausting all attempts
        /// </summary>
        public void MarkAsFailed(string? errorMessage = null)
        {
            if (Status != RetryStatus.InProgress)
                throw new InvalidOperationException("Only in-progress retries can be marked as failed");

            Status = RetryStatus.Failed;
            LastError = TruncateErrorMessage(errorMessage);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Truncates error message to maximum allowed length
        /// </summary>
        private static string? TruncateErrorMessage(string? errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return errorMessage;

            return errorMessage.Length > MaxErrorMessageLength
                ? errorMessage.Substring(0, MaxErrorMessageLength)
                : errorMessage;
        }
    }
}
