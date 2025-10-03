using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Repository interface for webhook log operations
    /// </summary>
    public interface IWebhookLogRepository
    {
        /// <summary>
        /// Gets webhook log by event ID
        /// </summary>
        /// <param name="eventId">Event ID from webhook provider</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Webhook log if found, null otherwise</returns>
        Task<PaymentWebhookLog?> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if webhook has already processed the payment with given event type
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="eventType">Event type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Webhook log if found, null otherwise</returns>
        Task<PaymentWebhookLog?> GetByPaymentAndEventAsync(Guid paymentId, string eventType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets webhook logs by provider with rate limiting
        /// </summary>
        /// <param name="provider">Payment gateway provider</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size (max 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of webhook logs</returns>
        Task<IEnumerable<PaymentWebhookLog>> GetByProviderAsync(
            string provider,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets webhook logs by event type with rate limiting
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size (max 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of webhook logs</returns>
        Task<IEnumerable<PaymentWebhookLog>> GetByEventTypeAsync(
            string eventType,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets failed webhook logs that can be retried
        /// </summary>
        /// <param name="maxAttempts">Maximum retry attempts</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of failed webhook logs</returns>
        Task<IEnumerable<PaymentWebhookLog>> GetFailedWebhooksAsync(
            int maxAttempts = 3, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new webhook log
        /// </summary>
        /// <param name="webhookLog">Webhook log to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added webhook log</returns>
        Task<PaymentWebhookLog> AddAsync(PaymentWebhookLog webhookLog, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing webhook log
        /// </summary>
        /// <param name="webhookLog">Webhook log to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAsync(PaymentWebhookLog webhookLog, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets webhook statistics
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Webhook statistics</returns>
        Task<WebhookStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Webhook statistics model
    /// </summary>
    public record WebhookStatistics
    {
        public int TotalWebhooks { get; init; }
        public int ProcessedWebhooks { get; init; }
        public int FailedWebhooks { get; init; }
        public int PendingWebhooks { get; init; }
        public Dictionary<string, int> WebhooksByProvider { get; init; } = new();
        public Dictionary<string, int> WebhooksByEventType { get; init; } = new();
        public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
    }
}
