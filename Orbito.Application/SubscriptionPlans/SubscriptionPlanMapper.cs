using Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.SubscriptionPlans;

/// <summary>
/// Centralized mapper for SubscriptionPlan entity to various DTOs.
/// Eliminates code duplication across handlers.
/// </summary>
public static class SubscriptionPlanMapper
{
    /// <summary>
    /// Maps SubscriptionPlan entity to full SubscriptionPlanDto with subscription counts.
    /// WARNING: Requires eager loading of Subscriptions navigation property to avoid N+1 queries.
    /// Use Include(p => p.Subscriptions) in repository query.
    /// </summary>
    public static SubscriptionPlanDto ToDto(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Amount = plan.Price.Amount,
            Currency = plan.Price.Currency,
            BillingPeriod = plan.BillingPeriod.ToString(),
            TrialPeriodDays = plan.TrialPeriodDays,
            FeaturesJson = plan.FeaturesJson,
            LimitationsJson = plan.LimitationsJson,
            IsActive = plan.IsActive,
            IsPublic = plan.IsPublic,
            SortOrder = plan.SortOrder,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            ActiveSubscriptionsCount = plan.Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0,
            TotalSubscriptionsCount = plan.Subscriptions?.Count ?? 0
        };
    }

    /// <summary>
    /// Maps SubscriptionPlan entity to SubscriptionPlanDto without subscription counts.
    /// Use when subscriptions are not loaded.
    /// </summary>
    public static SubscriptionPlanDto ToDtoWithoutCounts(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Amount = plan.Price.Amount,
            Currency = plan.Price.Currency,
            BillingPeriod = plan.BillingPeriod.ToString(),
            TrialPeriodDays = plan.TrialPeriodDays,
            FeaturesJson = plan.FeaturesJson,
            LimitationsJson = plan.LimitationsJson,
            IsActive = plan.IsActive,
            IsPublic = plan.IsPublic,
            SortOrder = plan.SortOrder,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            ActiveSubscriptionsCount = 0,
            TotalSubscriptionsCount = 0
        };
    }

    /// <summary>
    /// Maps SubscriptionPlan entity to ActiveSubscriptionPlanDto for public listing.
    /// WARNING: Requires eager loading of Subscriptions navigation property to avoid N+1 queries.
    /// Use Include(p => p.Subscriptions) in repository query.
    /// </summary>
    public static ActiveSubscriptionPlanDto ToActiveDto(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new ActiveSubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Amount = plan.Price.Amount,
            Currency = plan.Price.Currency,
            BillingPeriod = plan.BillingPeriod.ToString(),
            TrialPeriodDays = plan.TrialPeriodDays,
            FeaturesJson = plan.FeaturesJson,
            LimitationsJson = plan.LimitationsJson,
            SortOrder = plan.SortOrder,
            ActiveSubscriptionsCount = plan.Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0
        };
    }

    /// <summary>
    /// Maps SubscriptionPlan entity to SubscriptionPlanListItemDto for paginated listing.
    /// WARNING: Requires eager loading of Subscriptions navigation property to avoid N+1 queries.
    /// Use Include(p => p.Subscriptions) in repository query.
    /// </summary>
    public static SubscriptionPlanListItemDto ToListItemDto(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new SubscriptionPlanListItemDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Amount = plan.Price.Amount,
            Currency = plan.Price.Currency,
            BillingPeriod = plan.BillingPeriod.ToString(),
            TrialPeriodDays = plan.TrialPeriodDays,
            IsActive = plan.IsActive,
            IsPublic = plan.IsPublic,
            SortOrder = plan.SortOrder,
            CreatedAt = plan.CreatedAt,
            ActiveSubscriptionsCount = plan.Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0,
            TotalSubscriptionsCount = plan.Subscriptions?.Count ?? 0
        };
    }

    /// <summary>
    /// Maps collection of SubscriptionPlan entities to SubscriptionPlanDto list.
    /// </summary>
    public static List<SubscriptionPlanDto> ToDtos(IEnumerable<SubscriptionPlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);
        return plans.Select(ToDto).ToList();
    }

    /// <summary>
    /// Maps collection of SubscriptionPlan entities to ActiveSubscriptionPlanDto list.
    /// </summary>
    public static List<ActiveSubscriptionPlanDto> ToActiveDtos(IEnumerable<SubscriptionPlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);
        return plans.Select(ToActiveDto).ToList();
    }

    /// <summary>
    /// Maps collection of SubscriptionPlan entities to SubscriptionPlanListItemDto list.
    /// </summary>
    public static List<SubscriptionPlanListItemDto> ToListItemDtos(IEnumerable<SubscriptionPlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);
        return plans.Select(ToListItemDto).ToList();
    }
}
