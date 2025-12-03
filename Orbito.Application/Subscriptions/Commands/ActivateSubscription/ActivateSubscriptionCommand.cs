using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription
{
    public record ActivateSubscriptionCommand : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
    }
}
