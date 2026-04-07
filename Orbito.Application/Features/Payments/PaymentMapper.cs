using Orbito.Application.DTOs;
using Orbito.Domain.Entities;

namespace Orbito.Application.Features.Payments;

/// <summary>
/// Centralized mapper for Payment entity to DTO conversions.
/// Eliminates duplication across handlers.
/// </summary>
public static class PaymentMapper
{
    /// <summary>
    /// Maps a Payment entity to a PaymentDto.
    /// </summary>
    /// <param name="payment">The payment entity to map.</param>
    /// <returns>A PaymentDto containing the payment data.</returns>
    public static PaymentDto ToDto(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return new PaymentDto
        {
            Id = payment.Id,
            TenantId = payment.TenantId.Value,
            SubscriptionId = payment.SubscriptionId,
            ClientId = payment.ClientId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status.ToString(),
            ExternalTransactionId = payment.ExternalTransactionId,
            PaymentMethod = payment.PaymentMethod,
            ExternalPaymentId = payment.ExternalPaymentId,
            PaymentMethodId = payment.PaymentMethodId,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            FailedAt = payment.FailedAt,
            RefundedAt = payment.RefundedAt,
            FailureReason = payment.FailureReason
        };
    }

    /// <summary>
    /// Maps a collection of Payment entities to PaymentDtos.
    /// </summary>
    /// <param name="payments">The payment entities to map.</param>
    /// <returns>A read-only list of PaymentDtos.</returns>
    public static IReadOnlyList<PaymentDto> ToDto(IEnumerable<Payment> payments)
    {
        ArgumentNullException.ThrowIfNull(payments);

        return payments.Select(ToDto).ToList();
    }
}
