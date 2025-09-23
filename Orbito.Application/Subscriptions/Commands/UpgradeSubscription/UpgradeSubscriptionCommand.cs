using MediatR;

namespace Orbito.Application.Subscriptions.Commands.UpgradeSubscription
{
    public record UpgradeSubscriptionCommand : IRequest<UpgradeSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
        public Guid NewPlanId { get; init; }
        public decimal NewAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}
