using MediatR;

namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription
{
    public record SuspendSubscriptionCommand : IRequest<SuspendSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
        public string? Reason { get; init; }
    }
}
