using Orbito.Domain.Enums;

namespace Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions
{
    public record GetExpiringSubscriptionsResult
    {
        public List<ExpiringSubscriptionDto> Subscriptions { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public int DaysBeforeExpiration { get; init; }
    }

    public record ExpiringSubscriptionDto
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public SubscriptionStatus Status { get; init; }
        public decimal CurrentPrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime NextBillingDate { get; init; }
        public bool IsInTrial { get; init; }
        public DateTime? TrialEndDate { get; init; }
        public string? ClientCompanyName { get; init; }
        public string? ClientEmail { get; init; }
        public string? PlanName { get; init; }
        public int DaysUntilExpiration { get; init; }
    }
}
