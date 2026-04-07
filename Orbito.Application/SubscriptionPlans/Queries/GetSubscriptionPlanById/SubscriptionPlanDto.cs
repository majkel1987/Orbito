namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;

public record SubscriptionPlanDto
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
    public bool IsActive { get; init; }
    public bool IsPublic { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int ActiveSubscriptionsCount { get; init; }
    public int TotalSubscriptionsCount { get; init; }
}
