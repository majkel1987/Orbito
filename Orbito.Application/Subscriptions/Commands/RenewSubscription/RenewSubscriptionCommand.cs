using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public record RenewSubscriptionCommand : IRequest<Result<SubscriptionDto>>
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? ExternalPaymentId { get; init; }
    }
}
