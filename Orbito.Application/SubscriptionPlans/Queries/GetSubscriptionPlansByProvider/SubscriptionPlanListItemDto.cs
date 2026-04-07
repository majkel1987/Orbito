namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;

/// <summary>
/// Lightweight DTO for subscription plan list items in paginated results.
/// Used by GetSubscriptionPlansByProvider query for provider dashboard.
/// </summary>
public record SubscriptionPlanListItemDto
{
    /// <summary>
    /// Unique identifier of the subscription plan.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Display name of the subscription plan.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional detailed description of the plan features and benefits.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Price amount for the subscription period.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// ISO 4217 currency code (e.g., "USD", "EUR", "PLN").
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Billing period frequency (e.g., "Monthly", "Yearly").
    /// </summary>
    public string BillingPeriod { get; init; } = string.Empty;

    /// <summary>
    /// Number of free trial days before first charge.
    /// </summary>
    public int TrialPeriodDays { get; init; }

    /// <summary>
    /// Indicates whether the plan is currently active and available for new subscriptions.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indicates whether the plan is publicly visible to clients.
    /// </summary>
    public bool IsPublic { get; init; }

    /// <summary>
    /// Display order for sorting plans in UI (lower values appear first).
    /// </summary>
    public int SortOrder { get; init; }

    /// <summary>
    /// Timestamp when the plan was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Count of currently active subscriptions using this plan.
    /// </summary>
    public int ActiveSubscriptionsCount { get; init; }

    /// <summary>
    /// Total count of all subscriptions (active, cancelled, expired) using this plan.
    /// </summary>
    public int TotalSubscriptionsCount { get; init; }
}
