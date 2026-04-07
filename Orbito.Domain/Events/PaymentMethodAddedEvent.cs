using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Domain.Events;

/// <summary>
/// Domain event raised when a payment method is added
/// </summary>
public record PaymentMethodAddedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid PaymentMethodId { get; }
    public Guid ClientId { get; }
    public PaymentMethodType Type { get; }
    public bool IsDefault { get; }

    public PaymentMethodAddedEvent(
        Guid paymentMethodId,
        Guid clientId,
        PaymentMethodType type,
        bool isDefault,
        DateTime? occurredOn = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(paymentMethodId, Guid.Empty, nameof(paymentMethodId));
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty, nameof(clientId));

        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid payment method type");

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentMethodId = paymentMethodId;
        ClientId = clientId;
        Type = type;
        IsDefault = isDefault;
    }
}
