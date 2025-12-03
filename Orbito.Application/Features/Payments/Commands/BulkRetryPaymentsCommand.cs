using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Command to retry multiple failed payments in bulk
    /// </summary>
    public class BulkRetryPaymentsCommand : IRequest<Result<BulkRetryPaymentsResponse>>
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
}
