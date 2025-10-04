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
        if (paymentMethodId == Guid.Empty)
            throw new ArgumentException("PaymentMethodId cannot be empty", nameof(paymentMethodId));

        if (clientId == Guid.Empty)
            throw new ArgumentException("ClientId cannot be empty", nameof(clientId));

        if (!Enum.IsDefined(typeof(PaymentMethodType), type))
            throw new ArgumentException("Invalid payment method type", nameof(type));

        if (previousDefaultPaymentMethodId.HasValue &&
            previousDefaultPaymentMethodId.Value == Guid.Empty)
            throw new ArgumentException("PreviousDefaultPaymentMethodId cannot be empty Guid",
                nameof(previousDefaultPaymentMethodId));

        Id = Guid.NewGuid();
        OccurredOn = occurredOn ?? DateTime.UtcNow;
        PaymentMethodId = paymentMethodId;
        ClientId = clientId;
        Type = type;
        PreviousDefaultPaymentMethodId = previousDefaultPaymentMethodId;
    }
}
