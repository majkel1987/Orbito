using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbito.Infrastructure.PaymentGateways.Stripe.Models
{
    /// <summary>
    /// Stripe webhook data model
    /// </summary>
    public class StripeWebhookData
    {
        /// <summary>
        /// Unique identifier for the webhook event
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Type of the webhook event
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Time when the event was created (Unix timestamp)
        /// </summary>
        [JsonPropertyName("created")]
        public long Created { get; set; }

        /// <summary>
        /// Data object containing the event details
        /// </summary>
        [JsonPropertyName("data")]
        public StripeWebhookDataObject Data { get; set; } = new();

        /// <summary>
        /// API version of the webhook
        /// </summary>
        [JsonPropertyName("api_version")]
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Whether the event is in live mode
        /// </summary>
        [JsonPropertyName("livemode")]
        public bool Livemode { get; set; }

        /// <summary>
        /// Number of webhook delivery attempts
        /// </summary>
        [JsonPropertyName("pending_webhooks")]
        public int PendingWebhooks { get; set; }

        /// <summary>
        /// Request ID for idempotency
        /// </summary>
        [JsonPropertyName("request")]
        public StripeWebhookRequest? Request { get; set; }

        /// <summary>
        /// Gets the created date as DateTime
        /// </summary>
        [JsonIgnore]
        public DateTime CreatedDateTime => DateTimeOffset.FromUnixTimeSeconds(Created).UtcDateTime;
    }

    /// <summary>
    /// Stripe webhook data object
    /// </summary>
    public class StripeWebhookDataObject
    {
        /// <summary>
        /// The actual object that triggered the event
        /// </summary>
        [JsonPropertyName("object")]
        public JsonElement Object { get; set; }

        /// <summary>
        /// Previous attributes (for update events)
        /// </summary>
        [JsonPropertyName("previous_attributes")]
        public JsonElement? PreviousAttributes { get; set; }
    }

    /// <summary>
    /// Stripe webhook request information
    /// </summary>
    public class StripeWebhookRequest
    {
        /// <summary>
        /// Request ID
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Request ID for idempotency
        /// </summary>
        [JsonPropertyName("idempotency_key")]
        public string? IdempotencyKey { get; set; }
    }
}
