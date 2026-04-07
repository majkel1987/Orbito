using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Command to retry a failed payment
/// </summary>
public record RetryFailedPaymentCommand : IRequest<Result<RetryFailedPaymentResponse>>
{
    /// <summary>
    /// ID of the payment to retry
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// ID of the client requesting the retry
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Reason for the retry (optional)
    /// </summary>
    public string? Reason { get; init; }
}
