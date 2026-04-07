using Orbito.Domain.Common;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Interface for handling payment events (success/failure callbacks).
/// </summary>
public interface IPaymentEventHandler
{
    /// <summary>
    /// Handle successful payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> HandlePaymentSuccessAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handle failed payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="reason">Failure reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> HandlePaymentFailureAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);
}
