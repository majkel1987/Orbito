using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries
{
    /// <summary>
    /// Query to get scheduled retries with filtering and pagination
    /// </summary>
    public class GetScheduledRetriesQuery : IRequest<PaginatedList<RetryScheduleDto>>
    {
        /// <summary>
        /// Client ID to filter retries (optional)
        /// </summary>
        public Guid? ClientId { get; set; }

        /// <summary>
        /// Status to filter retries (optional)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Pagination parameters
        /// </summary>
        public PaginationParams Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO for retry schedule information
    /// </summary>
    public class RetryScheduleDto
    {
        /// <summary>
        /// ID of the retry schedule
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the payment
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// ID of the client
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// When the next retry attempt will be made
        /// </summary>
        public DateTime NextAttemptAt { get; set; }

        /// <summary>
        /// Current attempt number
        /// </summary>
        public int AttemptNumber { get; set; }

        /// <summary>
        /// Maximum number of attempts allowed
        /// </summary>
        public int MaxAttempts { get; set; }

        /// <summary>
        /// Current status of the retry
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Last error message
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// When the retry schedule was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the retry schedule was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Whether the retry is overdue
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Whether the retry can be attempted
        /// </summary>
        public bool CanRetry { get; set; }
    }
}
