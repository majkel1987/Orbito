using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public record ResumeSubscriptionCommand : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
    }
}
