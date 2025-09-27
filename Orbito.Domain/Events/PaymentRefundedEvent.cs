using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Events
{
    public record PaymentRefundedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public Guid PaymentId { get; }
        public Guid SubscriptionId { get; }
        public Guid ClientId { get; }
        public Money Amount { get; }
        public string RefundReason { get; }
        public string? ExternalTransactionId { get; }

        public PaymentRefundedEvent(
            Guid paymentId,
            Guid subscriptionId,
            Guid clientId,
            Money amount,
            string refundReason,
            string? externalTransactionId,
            DateTime? occurredOn = null)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentException("PaymentId cannot be empty", nameof(paymentId));

            if (subscriptionId == Guid.Empty)
                throw new ArgumentException("SubscriptionId cannot be empty", nameof(subscriptionId));

            if (clientId == Guid.Empty)
                throw new ArgumentException("ClientId cannot be empty", nameof(clientId));

            if (amount == null)
                throw new ArgumentNullException(nameof(amount));

            if (amount.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            if (string.IsNullOrWhiteSpace(refundReason))
                throw new ArgumentException("RefundReason cannot be empty", nameof(refundReason));

            Id = Guid.NewGuid();
            OccurredOn = occurredOn ?? DateTime.UtcNow;
            PaymentId = paymentId;
            SubscriptionId = subscriptionId;
            ClientId = clientId;
            Amount = amount;
            RefundReason = refundReason;
            ExternalTransactionId = externalTransactionId;
        }
    }
}