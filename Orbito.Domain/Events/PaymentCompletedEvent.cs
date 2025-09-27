using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Events
{
    public record PaymentCompletedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }
        public Guid PaymentId { get; }
        public Guid SubscriptionId { get; }
        public Guid ClientId { get; }
        public Money Amount { get; }
        public string? ExternalTransactionId { get; }

        public PaymentCompletedEvent(
            Guid paymentId,
            Guid subscriptionId,
            Guid clientId,
            Money amount,
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

            Id = Guid.NewGuid();
            OccurredOn = occurredOn ?? DateTime.UtcNow;
            PaymentId = paymentId;
            SubscriptionId = subscriptionId;
            ClientId = clientId;
            Amount = amount;
            ExternalTransactionId = externalTransactionId;
        }
    }
}