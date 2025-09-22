using MediatR;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public record CreateSubscriptionPlanCommand : IRequest<CreateSubscriptionPlanResult>
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "USD";
        public BillingPeriodType BillingPeriodType { get; init; }
        public int TrialDays { get; init; } = 0;
        public int TrialPeriodDays { get; init; } = 0;
        public string? FeaturesJson { get; init; }
        public string? LimitationsJson { get; init; }
        public bool IsPublic { get; init; } = true;
        public int SortOrder { get; init; } = 0;
    }
}
