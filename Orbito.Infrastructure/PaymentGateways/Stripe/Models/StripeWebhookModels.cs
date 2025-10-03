using System.Text.Json.Serialization;

namespace Orbito.Infrastructure.PaymentGateways.Stripe.Models
{
    /// <summary>
    /// Stripe Payment Intent model for webhooks
    /// </summary>
    public class StripePaymentIntent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("customer")]
        public string? Customer { get; set; }

        [JsonPropertyName("payment_method")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("last_payment_error")]
        public StripePaymentError? LastPaymentError { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [JsonPropertyName("created")]
        public long? Created { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Stripe Payment Error model
    /// </summary>
    public class StripePaymentError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("decline_code")]
        public string? DeclineCode { get; set; }

        [JsonPropertyName("charge")]
        public string? Charge { get; set; }
    }

    /// <summary>
    /// Stripe Charge model for webhooks
    /// </summary>
    public class StripeCharge
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("amount_refunded")]
        public long AmountRefunded { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("customer")]
        public string? Customer { get; set; }

        [JsonPropertyName("payment_intent")]
        public string? PaymentIntent { get; set; }

        [JsonPropertyName("refunded")]
        public bool Refunded { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Checks if this is a partial refund
        /// </summary>
        [JsonIgnore]
        public bool IsPartialRefund => AmountRefunded > 0 && AmountRefunded < Amount;

        /// <summary>
        /// Checks if this is a full refund
        /// </summary>
        [JsonIgnore]
        public bool IsFullRefund => Refunded && AmountRefunded >= Amount;
    }

    /// <summary>
    /// Stripe Subscription model for webhooks
    /// </summary>
    public class StripeSubscription
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("customer")]
        public string Customer { get; set; } = string.Empty;

        [JsonPropertyName("current_period_start")]
        public long CurrentPeriodStart { get; set; }

        [JsonPropertyName("current_period_end")]
        public long CurrentPeriodEnd { get; set; }

        [JsonPropertyName("cancel_at_period_end")]
        public bool CancelAtPeriodEnd { get; set; }

        [JsonPropertyName("canceled_at")]
        public long? CanceledAt { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [JsonPropertyName("items")]
        public StripeSubscriptionItems? Items { get; set; }

        /// <summary>
        /// Gets the current period start as DateTime
        /// </summary>
        [JsonIgnore]
        public DateTime CurrentPeriodStartDateTime =>
            DateTimeOffset.FromUnixTimeSeconds(CurrentPeriodStart).UtcDateTime;

        /// <summary>
        /// Gets the current period end as DateTime
        /// </summary>
        [JsonIgnore]
        public DateTime CurrentPeriodEndDateTime =>
            DateTimeOffset.FromUnixTimeSeconds(CurrentPeriodEnd).UtcDateTime;
    }

    /// <summary>
    /// Stripe Subscription Items wrapper
    /// </summary>
    public class StripeSubscriptionItems
    {
        [JsonPropertyName("data")]
        public List<StripeSubscriptionItem> Data { get; set; } = new();
    }

    /// <summary>
    /// Stripe Subscription Item
    /// </summary>
    public class StripeSubscriptionItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public StripePrice? Price { get; set; }
    }

    /// <summary>
    /// Stripe Price model
    /// </summary>
    public class StripePrice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("product")]
        public string Product { get; set; } = string.Empty;
    }

    /// <summary>
    /// Stripe Invoice model for webhooks
    /// </summary>
    public class StripeInvoice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("amount_paid")]
        public long AmountPaid { get; set; }

        [JsonPropertyName("amount_due")]
        public long AmountDue { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("customer")]
        public string Customer { get; set; } = string.Empty;

        [JsonPropertyName("subscription")]
        public string? Subscription { get; set; }

        [JsonPropertyName("payment_intent")]
        public string? PaymentIntent { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [JsonPropertyName("attempt_count")]
        public int? AttemptCount { get; set; }

        [JsonPropertyName("billing_reason")]
        public string? BillingReason { get; set; }
    }

    /// <summary>
    /// Stripe Refund model for webhooks
    /// </summary>
    public class StripeRefund
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("charge")]
        public string? Charge { get; set; }

        [JsonPropertyName("payment_intent")]
        public string? PaymentIntent { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
