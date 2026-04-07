using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Domain.Events;

/// <summary>
/// Domain event raised when a payment method is set as default
/// </summary>
public record PaymentMethodSetAsDefaultEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid PaymentMethodId { get; }
    public Guid ClientId { get; }
    public PaymentMethodType Type { get; }
    public Guid? PreviousDefaultPaymentMethodId { get; }

    public PaymentMethodSetAsDefaultEvent(
        Guid paymentMethodId,
        Guid clientId,
        PaymentMethodType type,
        Guid? previousDefaultPaymentMethodId = null,
        DateTime? occurredOn = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(paymentMethodId, Guid.Empty, nameof(paymentMethodId));
        ArgumentOutOfRangeException.ThrowIfEqual(clientId, Guid.Empty, nameof(clientId));

        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid payment method type");

        if (previousDefaultPaymentMethodId.HasValue &&
            previousDefaultPaymentMethodId.Value == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(previousDefaultPaymentMethodId),
                "PreviousDefaultPaymentMethodId cannot be empty Guid");

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentMethodId = paymentMethodId;
        ClientId = clientId;
        Type = type;
        PreviousDefaultPaymentMethodId = previousDefaultPaymentMethodId;
    }
}
