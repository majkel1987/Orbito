using Orbito.Application.Common.Models.PaymentGateway;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Payment gateway abstraction — enables easy switching between different payment providers (Stripe, PayPal, etc.).
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Processes a payment through the payment gateway.
    /// </summary>
    /// <param name="request">Payment data</param>
    /// <returns>Payment processing result</returns>
    Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);

    /// <summary>
    /// Creates a PaymentIntent for Stripe Elements (client-side confirmation).
    /// PCI DSS compliant — card data never touches our servers.
    /// </summary>
    /// <param name="request">Payment intent creation request</param>
    /// <returns>Result with client secret for frontend</returns>
    Task<CreatePaymentIntentResult> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);

    /// <summary>
    /// Refunds a payment through the payment gateway.
    /// </summary>
    /// <param name="request">Refund data</param>
    /// <returns>Refund result</returns>
    Task<RefundResult> RefundPaymentAsync(RefundRequest request);

    /// <summary>
    /// Creates a customer in the payment gateway.
    /// </summary>
    /// <param name="request">Customer data</param>
    /// <returns>Customer creation result</returns>
    Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request);

    /// <summary>
    /// Gets the payment status from the payment gateway.
    /// </summary>
    /// <param name="externalPaymentId">External payment ID</param>
    /// <returns>Payment status</returns>
    Task<PaymentStatusResult> GetPaymentStatusAsync(string externalPaymentId);

    /// <summary>
    /// Validates a webhook from the payment gateway.
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <returns>Webhook validation result with details</returns>
    Task<WebhookValidationResult> ValidateWebhookAsync(string payload, string signature);
}
