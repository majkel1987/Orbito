using System.ComponentModel.DataAnnotations;

namespace Orbito.API.Models
{
    /// <summary>
    /// Stripe webhook request model
    /// </summary>
    public class StripeWebhookRequest
    {
        /// <summary>
        /// Event type from Stripe
        /// </summary>
        [Required]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event ID from Stripe
        /// </summary>
        [Required]
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Raw webhook payload
        /// </summary>
        [Required]
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Stripe signature for verification
        /// </summary>
        [Required]
        public string Signature { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        public long Created { get; set; }

        /// <summary>
        /// Whether the event is in live mode
        /// </summary>
        public bool Livemode { get; set; }

        /// <summary>
        /// API version of the webhook
        /// </summary>
        public string? ApiVersion { get; set; }
    }


    /// <summary>
    /// Webhook processing result
    /// </summary>
    public class WebhookProcessingResult
    {
        /// <summary>
        /// Whether the webhook was processed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Event ID from the webhook
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Event type
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Processing message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Processing timestamp
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Processing duration in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }
    }
}
