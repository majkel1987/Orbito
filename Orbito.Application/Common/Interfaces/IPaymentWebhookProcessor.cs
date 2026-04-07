using Orbito.Domain.Common;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Interface for processing payment webhooks from external payment gateways.
/// Handles signature validation, idempotency, and event processing.
/// </summary>
public interface IPaymentWebhookProcessor
{
    /// <summary>
    /// Processes a webhook event from payment gateway.
    /// </summary>
    /// <param name="eventType">Type of webhook event</param>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of webhook processing</returns>
    Task<Result> ProcessWebhookEventAsync(string eventType, string payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates webhook signature.
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<Result<bool>> ValidateWebhookSignatureAsync(string payload, string signature, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if webhook event is idempotent (already processed).
    /// </summary>
    /// <param name="eventId">Unique event ID from webhook</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if event was already processed</returns>
    Task<Result<bool>> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks webhook event as processed.
    /// </summary>
    /// <param name="eventId">Unique event ID from webhook</param>
    /// <param name="eventType">Type of webhook event</param>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of marking event as processed</returns>
    Task<Result> MarkEventAsProcessedAsync(string eventId, string eventType, string payload, CancellationToken cancellationToken = default);
}
