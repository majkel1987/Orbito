using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Interface for processing payment refunds.
/// </summary>
public interface IRefundService
{
    /// <summary>
    /// Refund a payment (full or partial).
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="amount">Refund amount</param>
    /// <param name="reason">Refund reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refund result</returns>
    Task<Result<RefundResult>> RefundPaymentAsync(
        Guid paymentId,
        Money amount,
        string reason,
        CancellationToken cancellationToken = default);
}
