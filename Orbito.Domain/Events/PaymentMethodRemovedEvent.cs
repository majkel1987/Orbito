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
        ArgumentOutOfRangeException.ThrowIfEqual(paymentMethodId, Guid.Empty, nameof(paymentMethodId));
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty, nameof(clientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid payment method type");

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentMethodId = paymentMethodId;
        ClientId = clientId;
        Type = type;
        Reason = reason;
    }
}
