using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Commands.DowngradeSubscription
{
    public record DowngradeSubscriptionCommand : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public Guid NewPlanId { get; init; }
        public decimal NewAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}
