using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.ProcessPayment;

/// <summary>
/// Command to process a payment
/// </summary>
public record ProcessPaymentCommand : IRequest<Result<PaymentDto>>
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public required Guid SubscriptionId { get; init; }

    /// <summary>
    /// Client ID
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Payment amount
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// External transaction ID
    /// </summary>
    public string? ExternalTransactionId { get; init; }

    /// <summary>
    /// Payment method
    /// </summary>
    public string? PaymentMethod { get; init; }

    /// <summary>
    /// External payment ID
    /// </summary>
    public string? ExternalPaymentId { get; init; }
}
