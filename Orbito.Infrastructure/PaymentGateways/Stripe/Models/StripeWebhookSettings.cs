namespace Orbito.Infrastructure.PaymentGateways.Stripe.Models
{
    /// <summary>
    /// Stripe webhook settings
    /// </summary>
    public class StripeWebhookSettings
    {
        /// <summary>
        /// Stripe webhook secret for signature verification
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;

        /// <summary>
        /// Whether to enable signature verification
        /// </summary>
        public bool EnableSignatureVerification { get; set; } = true;

        /// <summary>
        /// Maximum payload size in bytes
        /// </summary>
        public int MaxPayloadSize { get; set; } = 1024 * 1024; // 1MB

        /// <summary>
        /// Whether to log webhook payloads (be careful with PII/sensitive data)
        /// </summary>
        public bool LogPayloads { get; set; } = false;

        /// <summary>
        /// Webhook timeout tolerance in seconds (Stripe recommends 300s for signature verification)
        /// </summary>
        public long SignatureToleranceSeconds { get; set; } = 300;

        /// <summary>
        /// Maximum number of retry attempts for failed webhook processing
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Enable asynchronous processing (quick ACK pattern)
        /// </summary>
        public bool EnableAsyncProcessing { get; set; } = true;
    }
}