using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    /// <summary>
    /// Entity for logging payment webhook events
    /// </summary>
    public class PaymentWebhookLog : IMustHaveTenant
    {
        /// <summary>
        /// Unique identifier for the webhook log
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public TenantId TenantId { get; private set; }

        /// <summary>
        /// Unique event ID from the webhook provider
        /// </summary>
        public string EventId { get; private set; } = string.Empty;

        /// <summary>
        /// Payment gateway provider (e.g., Stripe, PayPal)
        /// </summary>
        public string Provider { get; private set; } = string.Empty;

        /// <summary>
        /// Type of webhook event
        /// </summary>
        public string EventType { get; private set; } = string.Empty;

        /// <summary>
        /// Raw webhook payload
        /// </summary>
        public string Payload { get; private set; } = string.Empty;

        /// <summary>
        /// When the webhook was processed
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Processing status (Pending, Processed, Failed)
        /// </summary>
        public WebhookStatus Status { get; private set; }

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Number of processing attempts
        /// </summary>
        public int Attempts { get; private set; } = 1;

        /// <summary>
        /// When the webhook was first received
        /// </summary>
        public DateTime ReceivedAt { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata for the webhook
        /// </summary>
        public string? Metadata { get; private set; }

        /// <summary>
        /// Creates a new webhook log entry
        /// </summary>
        public static PaymentWebhookLog Create(
            TenantId tenantId,
            string eventId,
            string provider,
            string eventType,
            string payload)
        {
            return new PaymentWebhookLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EventId = eventId,
                Provider = provider,
                EventType = eventType,
                Payload = payload,
                Status = WebhookStatus.Pending,
                ReceivedAt = DateTime.UtcNow,
                Attempts = 0
            };
        }

        /// <summary>
        /// Creates a new webhook log entry without tenant context (for system-wide webhooks)
        /// </summary>
        public static PaymentWebhookLog CreateForWebhook(
            string eventId,
            string provider,
            string eventType,
            string payload,
            WebhookStatus status = WebhookStatus.Pending)
        {
            return new PaymentWebhookLog
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId.Empty,
                EventId = eventId,
                Provider = provider,
                EventType = eventType,
                Payload = payload,
                Status = status,
                ReceivedAt = DateTime.UtcNow,
                Attempts = 1
            };
        }

        /// <summary>
        /// Marks the webhook as failed
        /// </summary>
        public void MarkAsFailed(string errorMessage)
        {
            Status = WebhookStatus.Failed;
            ErrorMessage = errorMessage;
            Attempts++;
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the webhook as processed
        /// </summary>
        public void MarkAsProcessed()
        {
            Status = WebhookStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
            Attempts++;
        }

        /// <summary>
        /// Checks if the webhook can be retried
        /// </summary>
        public bool CanRetry(int maxAttempts = 3)
        {
            return Status == WebhookStatus.Failed && Attempts < maxAttempts;
        }
    }
}
