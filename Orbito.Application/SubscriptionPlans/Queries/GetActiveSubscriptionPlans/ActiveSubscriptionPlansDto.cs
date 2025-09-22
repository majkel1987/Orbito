namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
    public record ActiveSubscriptionPlansDto
    {
        public List<ActiveSubscriptionPlanDto> Plans { get; init; } = new();
        public int TotalCount { get; init; }
    }

    public record ActiveSubscriptionPlanDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BillingPeriod { get; init; } = string.Empty;
        public int TrialPeriodDays { get; init; }
        public string? FeaturesJson { get; init; }
        public string? LimitationsJson { get; init; }
        public int SortOrder { get; init; }
        public int ActiveSubscriptionsCount { get; init; }
    }
}
