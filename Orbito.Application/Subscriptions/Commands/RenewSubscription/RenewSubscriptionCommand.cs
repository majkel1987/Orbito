using MediatR;

namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public record RenewSubscriptionCommand : IRequest<RenewSubscriptionResult>
    {
        public Guid SubscriptionId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? ExternalPaymentId { get; init; }
    }
}
