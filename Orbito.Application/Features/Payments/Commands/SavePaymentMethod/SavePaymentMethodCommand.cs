using MediatR;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.SavePaymentMethod;

/// <summary>
/// Command for saving payment method
/// </summary>
public record SavePaymentMethodCommand : IRequest<Result<SavePaymentMethodResult>>
{
    /// <summary>
    /// Client ID
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Payment method type
    /// </summary>
    public required PaymentMethodType Type { get; init; }

    /// <summary>
    /// Encrypted payment method token
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Last four digits of the card
    /// </summary>
    public string? LastFourDigits { get; init; }

    /// <summary>
    /// Expiry date of the payment method
    /// </summary>
    public DateTime? ExpiryDate { get; init; }

    /// <summary>
    /// Whether this is the default payment method
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Result for saving payment method
/// </summary>
public record SavePaymentMethodResult
{
    /// <summary>
    /// Payment method ID
    /// </summary>
    public required Guid PaymentMethodId { get; init; }
}
