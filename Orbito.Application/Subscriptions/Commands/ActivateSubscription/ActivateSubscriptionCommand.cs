using MediatR;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription
{
    public record ActivateSubscriptionCommand : IRequest<ActivateSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
    }
}
