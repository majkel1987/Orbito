using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription
{
    public record CreateSubscriptionCommand : IRequest<CreateSubscriptionResult>
    {
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int BillingPeriodValue { get; init; }
        public string BillingPeriodType { get; init; } = string.Empty;
        public int TrialDays { get; init; } = 0;
    }
}
