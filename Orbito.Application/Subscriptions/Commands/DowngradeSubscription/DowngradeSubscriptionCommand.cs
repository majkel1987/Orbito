using MediatR;

namespace Orbito.Application.Subscriptions.Commands.DowngradeSubscription
{
    public record DowngradeSubscriptionCommand : IRequest<DowngradeSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
        public Guid NewPlanId { get; init; }
        public decimal NewAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}
