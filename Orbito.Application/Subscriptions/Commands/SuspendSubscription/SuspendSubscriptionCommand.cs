using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription
{
    public record SuspendSubscriptionCommand : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public string? Reason { get; init; }
    }
}
