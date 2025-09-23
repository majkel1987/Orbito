using Orbito.Domain.Enums;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionById
{
    public record GetSubscriptionByIdResult
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public SubscriptionStatus Status { get; init; }
        public decimal CurrentPrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BillingPeriod { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public DateTime NextBillingDate { get; init; }
        public bool IsInTrial { get; init; }
        public DateTime? TrialEndDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? CancelledAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        // Client details (if IncludeDetails is true)
        public string? ClientCompanyName { get; init; }
        public string? ClientEmail { get; init; }
        public string? ClientFirstName { get; init; }
        public string? ClientLastName { get; init; }

        // Plan details (if IncludeDetails is true)
        public string? PlanName { get; init; }
        public string? PlanDescription { get; init; }

        // Payment details (if IncludeDetails is true)
        public int PaymentCount { get; init; }
        public decimal TotalPaid { get; init; }
        public DateTime? LastPaymentDate { get; init; }
    }
}
