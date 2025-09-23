using Orbito.Domain.Enums;

namespace Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions
{
    public record GetActiveSubscriptionsResult
    {
        public List<ActiveSubscriptionDto> Subscriptions { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }

    public record ActiveSubscriptionDto
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public SubscriptionStatus Status { get; init; }
        public decimal CurrentPrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BillingPeriod { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime NextBillingDate { get; init; }
        public bool IsInTrial { get; init; }
        public DateTime? TrialEndDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? ClientCompanyName { get; init; }
        public string? ClientEmail { get; init; }
        public string? PlanName { get; init; }
        public string? PlanDescription { get; init; }
    }
}
