using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Domain.Events;

/// <summary>
/// Domain event raised when a payment method is removed
/// </summary>
public record PaymentMethodRemovedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid PaymentMethodId { get; }
    public Guid ClientId { get; }
    public PaymentMethodType Type { get; }
    public string Reason { get; }

    public PaymentMethodRemovedEvent(
        Guid paymentMethodId,
        Guid clientId,
        PaymentMethodType type,
        string reason,
        DateTime? occurredOn = null)
    {
        if (paymentMethodId == Guid.Empty)
            throw new ArgumentException("PaymentMethodId cannot be empty", nameof(paymentMethodId));

        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty", nameof(clientId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty", nameof(reason));

        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentException("Invalid payment method type", nameof(type));

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentMethodId = paymentMethodId;
        ClientId = clientId;
        Type = type;
        Reason = reason;
    }
}
