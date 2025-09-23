using MediatR;

namespace Orbito.Application.Subscriptions.Commands.CancelSubscription
{
    public record CancelSubscriptionCommand : IRequest<CancelSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
        public string? Reason { get; init; }
    }
}
