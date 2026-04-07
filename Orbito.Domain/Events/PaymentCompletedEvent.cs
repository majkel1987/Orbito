using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Events;

/// <summary>
/// Domain event raised when a payment is completed successfully
/// </summary>
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
        ArgumentOutOfRangeException.ThrowIfEqual(paymentId, Guid.Empty, nameof(paymentId));
        ArgumentOutOfRangeException.ThrowIfEqual(subscriptionId, Guid.Empty, nameof(subscriptionId));
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty, nameof(clientId));
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero");

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentId = paymentId;
        SubscriptionId = subscriptionId;
        ClientId = clientId;
        Amount = amount;
        ExternalTransactionId = externalTransactionId;
    }
}
