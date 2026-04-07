using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Interface for processing payments through payment gateway.
/// </summary>
public interface IPaymentProcessor
{
    /// <summary>
    /// Process a subscription payment.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentMethodId">Payment method ID (external gateway ID)</param>
    /// <param name="description">Payment description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment processing result</returns>
    Task<Result<PaymentResult>> ProcessSubscriptionPaymentAsync(
        Guid subscriptionId,
        Money amount,
        string paymentMethodId,
        string description,
        CancellationToken cancellationToken = default);
}
