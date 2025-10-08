using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Command to retry a failed payment
    /// </summary>
    public class RetryFailedPaymentCommand : IRequest<RetryFailedPaymentResult>
    {
        /// <summary>
        /// ID of the payment to retry
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// ID of the client requesting the retry
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Reason for the retry (optional)
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Result of retry failed payment command
    /// </summary>
    public class RetryFailedPaymentResult
    {
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

        /// <summary>
        /// Maximum number of attempts allowed
        /// </summary>
        public int MaxAttempts { get; set; }
    }
}
