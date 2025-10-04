using Orbito.Domain.ValueObjects;
using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Service interface for processing payments
    /// </summary>
    public interface IPaymentProcessingService
    {
        /// <summary>
        /// Processes subscription payment
        /// </summary>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="amount">Payment amount</param>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <param name="description">Payment description</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment processing result</returns>
        Task<PaymentResult> ProcessSubscriptionPaymentAsync(
            Guid subscriptionId,
            Money amount,
            Guid paymentMethodId,
            string description,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles successful payment
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task HandlePaymentSuccessAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles failed payment
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="reason">Failure reason</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task HandlePaymentFailureAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if payment can be refunded
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="amount">Refund amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if refund is possible, false otherwise</returns>
        Task<bool> CanRefundAsync(Guid paymentId, Money amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refunds payment (full or partial)
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="amount">Refund amount</param>
        /// <param name="reason">Refund reason</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund result</returns>
        Task<RefundResult> RefundPaymentAsync(
            Guid paymentId,
            Money amount,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a customer in payment gateway
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="email">Client email</param>
        /// <param name="firstName">Client first name</param>
        /// <param name="lastName">Client last name</param>
        /// <param name="companyName">Company name</param>
        /// <param name="phone">Client phone number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer creation result</returns>
        Task<CustomerResult> CreateCustomerAsync(
            Guid clientId,
            string email,
            string? firstName = null,
            string? lastName = null,
            string? companyName = null,
            string? phone = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes pending payments
        /// </summary>
        /// <param name="billingDate">Billing date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ProcessPendingPaymentsAsync(DateTime billingDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates payment from webhook data with signature verification
        /// </summary>
        /// <param name="webhookData">Webhook data</param>
        /// <param name="stripeSignature">Stripe signature for verification</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdatePaymentFromWebhookAsync(string webhookData, string stripeSignature, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates payment status
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ValidatePaymentStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronizes payment statuses with Stripe
        /// </summary>
        /// <param name="syncDate">Synchronization date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SyncPaymentStatusesWithStripeAsync(DateTime syncDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks rate limit for client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Wait time or null if no rate limit applies</returns>
        Task<TimeSpan?> GetRateLimitDelayAsync(Guid clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default payment method ID for client
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Default payment method ID or null if not found</returns>
        Task<Guid?> GetDefaultPaymentMethodAsync(Guid clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates if payment method exists and can be used
        /// </summary>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <param name="clientId">Client ID for security verification</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if valid and usable, false otherwise</returns>
        Task<bool> ValidatePaymentMethodAsync(Guid paymentMethodId, Guid clientId, CancellationToken cancellationToken = default);
    }
}