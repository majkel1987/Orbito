namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan
{
    public record CloneSubscriptionPlanResult
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BillingPeriod { get; init; } = string.Empty;
        public int TrialPeriodDays { get; init; }
        public bool IsActive { get; init; }
        public bool IsPublic { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public Guid OriginalPlanId { get; init; }
    }
}
