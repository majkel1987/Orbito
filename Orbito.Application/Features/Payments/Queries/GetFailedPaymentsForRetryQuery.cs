using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries
{
    /// <summary>
    /// Query to get failed payments that can be retried
    /// </summary>
    public class GetFailedPaymentsForRetryQuery : IRequest<PaginatedList<FailedPaymentDto>>
    {
        /// <summary>
        /// Client ID to filter payments (optional)
        /// </summary>
        public Guid? ClientId { get; set; }

        /// <summary>
        /// Pagination parameters
        /// </summary>
        public PaginationParams Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO for failed payment information
    /// </summary>
    public class FailedPaymentDto
    {
        /// <summary>
        /// ID of the payment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the client
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// ID of the subscription
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// When the payment was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the payment failed
        /// </summary>
        public DateTime? FailedAt { get; set; }

        /// <summary>
        /// Reason for the failure
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// External transaction ID
        /// </summary>
        public string? ExternalTransactionId { get; set; }

        /// <summary>
        /// Payment method used
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Whether the payment has an active retry schedule
        /// </summary>
        public bool HasActiveRetry { get; set; }

        /// <summary>
        /// Number of retry attempts made
        /// </summary>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Whether the payment can be retried
        /// </summary>
        public bool CanRetry { get; set; }
    }
}
