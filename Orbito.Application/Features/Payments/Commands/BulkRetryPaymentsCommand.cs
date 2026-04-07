using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Command to retry multiple failed payments in bulk
/// </summary>
public record BulkRetryPaymentsCommand : IRequest<Result<BulkRetryPaymentsResponse>>
{
    /// <summary>
    /// List of payment IDs to retry
    /// </summary>
    public required List<Guid> PaymentIds { get; init; }

    /// <summary>
    /// Client ID requesting the bulk retry (security)
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Reason for the bulk retry (optional)
    /// </summary>
    public string? Reason { get; init; }
}
