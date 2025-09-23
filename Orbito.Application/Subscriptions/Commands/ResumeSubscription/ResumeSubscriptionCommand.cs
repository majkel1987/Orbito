using MediatR;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public record ResumeSubscriptionCommand : IRequest<ResumeSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
    }
}
