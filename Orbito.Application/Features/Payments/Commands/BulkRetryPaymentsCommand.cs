using MediatR;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Command to retry multiple failed payments in bulk
    /// </summary>
    public class BulkRetryPaymentsCommand : IRequest<BulkRetryPaymentsResult>
    {
        /// <summary>
        /// List of payment IDs to retry
        /// </summary>
        public List<Guid> PaymentIds { get; set; } = new();

        /// <summary>
        /// ID of the client requesting the bulk retry
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Reason for the bulk retry (optional)
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Result of bulk retry payments command
    /// </summary>
    public class BulkRetryPaymentsResult
    {
        /// <summary>
        /// Total number of payments processed
        /// </summary>
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Number of successful retry schedules created
        /// </summary>
        public int SuccessfulRetries { get; set; }

        /// <summary>
        /// Number of failed retry attempts
        /// </summary>
        public int FailedRetries { get; set; }

        /// <summary>
        /// List of individual retry results
        /// </summary>
        public List<BulkRetryItemResult> Results { get; set; } = new();

        /// <summary>
        /// Whether the bulk operation was successful
        /// </summary>
        public bool Success => FailedRetries == 0;
    }

    /// <summary>
    /// Result for individual payment retry in bulk operation
    /// </summary>
    public class BulkRetryItemResult
    {
        /// <summary>
        /// ID of the payment
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Whether the retry was scheduled successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID of the created retry schedule
        /// </summary>
        public Guid? RetryScheduleId { get; set; }

        /// <summary>
        /// Error message if retry failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When the next retry attempt will be made
        /// </summary>
        public DateTime? NextAttemptAt { get; set; }

        /// <summary>
        /// Current attempt number
        /// </summary>
        public int AttemptNumber { get; set; }
    }
}
