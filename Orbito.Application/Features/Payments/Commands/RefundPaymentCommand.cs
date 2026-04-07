using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Command to refund a payment
/// </summary>
public record RefundPaymentCommand : IRequest<Result<RefundPaymentResult>>
{
    /// <summary>
    /// Payment ID
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// Client ID (security: ClientId verification)
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Refund amount
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Refund reason
    /// </summary>
    public required string Reason { get; init; }
}

/// <summary>
/// Result of refund payment command
/// </summary>
public record RefundPaymentResult
{
    /// <summary>
    /// External refund ID from payment gateway
    /// </summary>
    public required string ExternalRefundId { get; init; }

    /// <summary>
    /// Refund status
    /// </summary>
    public required string Status { get; init; }
}
