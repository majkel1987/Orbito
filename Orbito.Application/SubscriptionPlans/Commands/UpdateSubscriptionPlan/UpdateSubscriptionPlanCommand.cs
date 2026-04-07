using MediatR;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand : IRequest<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public BillingPeriodType BillingPeriodType { get; init; }
    public int TrialDays { get; init; } = 0;
    public int TrialPeriodDays { get; init; } = 0;
    public string? FeaturesJson { get; init; }
    public string? LimitationsJson { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPublic { get; init; } = true;
    public int SortOrder { get; init; } = 0;
}
