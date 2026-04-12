using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Infrastructure.PaymentGateways.Stripe.EventHandlers;
using Orbito.Infrastructure.PaymentGateways.Stripe.Extensions;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using Stripe;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Orbito.Infrastructure.PaymentGateways.Stripe
{
    /// <summary>
    /// Stripe webhook processor implementation
    /// </summary>
    public class StripeWebhookProcessor : IPaymentWebhookProcessor
    {
        private readonly StripeConfiguration _configuration;
        private readonly StripeWebhookSettings _webhookSettings;
        private readonly ILogger<StripeWebhookProcessor> _logger;
        private readonly IWebhookLogRepository _webhookLogRepository;
        private readonly StripeEventHandler _eventHandler;
        private readonly IBackgroundJobQueue? _backgroundJobQueue; // Optional for async processing

        public StripeWebhookProcessor(
            IOptions<StripeConfiguration> configuration,
            IOptions<StripeWebhookSettings> webhookSettings,
            ILogger<StripeWebhookProcessor> logger,
            IWebhookLogRepository webhookLogRepository,
            StripeEventHandler eventHandler,
            IBackgroundJobQueue? backgroundJobQueue = null)
        {
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
            _webhookSettings = webhookSettings?.Value ?? throw new ArgumentNullException(nameof(webhookSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webhookLogRepository = webhookLogRepository ?? throw new ArgumentNullException(nameof(webhookLogRepository));
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
            _backgroundJobQueue = backgroundJobQueue;
        }

        /// <summary>
        /// Processes a webhook event from Stripe
        /// </summary>
        public async Task<Result> ProcessWebhookEventAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Validate payload size
                if (payload.Length > _webhookSettings.MaxPayloadSize)
                {
                    _logger.LogWarning("Webhook payload size {Size} exceeds maximum {MaxSize}",
                        payload.Length, _webhookSettings.MaxPayloadSize);
                    return Result.Failure(DomainErrors.Webhook.PayloadTooLarge);
                }

                _logger.LogInformation("Processing Stripe webhook event: {EventType}, Payload size: {Size} bytes",
                    eventType, payload.Length);

                // Parse the webhook payload
                StripeWebhookData? webhookData;
                try
                {
                    webhookData = JsonSerializer.Deserialize<StripeWebhookData>(payload);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse Stripe webhook payload");
                    await LogFailedWebhookAsync(null, eventType, payload, "Invalid JSON format", cancellationToken);
                    return Result.Failure(DomainErrors.Webhook.InvalidPayloadFormat);
                }

                if (webhookData == null)
                {
                    _logger.LogError("Webhook data deserialized to null");
                    await LogFailedWebhookAsync(null, eventType, payload, "Null webhook data", cancellationToken);
                    return Result.Failure(DomainErrors.Webhook.InvalidPayloadFormat);
                }

                // Validate event type matches
                if (!string.Equals(webhookData.Type, eventType, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Event type mismatch: header={HeaderType}, payload={PayloadType}",
                        eventType, webhookData.Type);
                }

                // Check for idempotency (prevent duplicate processing)
                var idempotencyCheck = await IsEventProcessedAsync(webhookData.Id, cancellationToken);
                if (!idempotencyCheck.IsSuccess)
                {
                    _logger.LogWarning("Failed to check idempotency for event {EventId}: {Error}",
                        webhookData.Id, idempotencyCheck.Error.Message);
                    // Continue processing - don't fail due to idempotency check issues
                }
                else if (idempotencyCheck.Value)
                {
                    _logger.LogInformation("Webhook event {EventId} already processed, skipping (idempotent)", webhookData.Id);
                    return Result.Success();
                }

                // Log webhook as received (before processing)
                await LogReceivedWebhookAsync(webhookData.Id, eventType, payload, cancellationToken);

                Result processResult;

                // Decide on processing strategy: sync or async
                if (_webhookSettings.EnableAsyncProcessing && _backgroundJobQueue != null)
                {
                    // Quick ACK pattern - enqueue for background processing
                    _backgroundJobQueue.EnqueueWebhookProcessing(webhookData.Id, eventType, payload);
                    _logger.LogInformation("Webhook event {EventId} enqueued for async processing", webhookData.Id);
                    processResult = Result.Success();
                }
                else
                {
                    // Synchronous processing
                    processResult = await ProcessEventInternalAsync(webhookData, eventType, payload, cancellationToken);
                }

                var processingTime = DateTime.UtcNow - startTime;
                _logger.LogInformation("Webhook processing completed for {EventId} in {ProcessingTime}ms",
                    webhookData.Id, processingTime.TotalMilliseconds);

                return processResult;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Webhook processing cancelled for event type: {EventType}", eventType);
                throw;
            }
            catch (Exception ex)
            {
                var processingTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Unexpected error processing Stripe webhook event: {EventType} (took {ProcessingTime}ms)",
                    eventType, processingTime.TotalMilliseconds);
                return Result.Failure(DomainErrors.General.UnexpectedError);
            }
        }

        /// <summary>
        /// Internal method to process event (can be called sync or async)
        /// </summary>
        private async Task<Result> ProcessEventInternalAsync(
            StripeWebhookData webhookData,
            string eventType,
            string payload,
            CancellationToken cancellationToken)
        {
            try
            {
                // Process the event based on type
                var processResult = await _eventHandler.HandleEventAsync(eventType, webhookData, cancellationToken);

                if (!processResult.IsSuccess)
                {
                    _logger.LogError("Failed to process webhook event {EventId}: {Error}",
                        webhookData.Id, processResult.Error.Message);
                    await LogFailedWebhookAsync(webhookData.Id, eventType, payload, processResult.Error.Message, cancellationToken);
                    return processResult;
                }

                // Mark event as successfully processed
                var markResult = await MarkEventAsProcessedAsync(webhookData.Id, eventType, payload, cancellationToken);
                if (!markResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to mark webhook event {EventId} as processed: {Error}",
                        webhookData.Id, markResult.Error.Message);
                    // Don't fail the whole operation if we can't mark as processed
                }

                _logger.LogInformation("Successfully processed Stripe webhook event: {EventId} (Type: {EventType})",
                    webhookData.Id, eventType);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in internal webhook processing for {EventId}", webhookData.Id);
                await LogFailedWebhookAsync(webhookData.Id, eventType, payload, ex.Message, cancellationToken);
                return Result.Failure(DomainErrors.General.UnexpectedError);
            }
        }

        /// <summary>
        /// Validates Stripe webhook signature using Stripe's official library
        /// </summary>
        public async Task<Result<bool>> ValidateWebhookSignatureAsync(
            string payload,
            string signature,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if signature verification is enabled
                if (!_webhookSettings.EnableSignatureVerification)
                {
                    _logger.LogWarning("Webhook signature verification is DISABLED - this is insecure for production!");
                    return Result<bool>.Success(true);
                }

                // Validate configuration
                if (string.IsNullOrEmpty(_configuration.WebhookSecret))
                {
                    _logger.LogError("Stripe webhook secret not configured - cannot verify signature");
                    return Result.Failure<bool>(DomainErrors.Webhook.SecretNotConfigured);
                }

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing webhook signature header");
                    return Result.Failure<bool>(DomainErrors.Webhook.MissingSignature);
                }

                // Use Stripe's official signature verification
                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(
                        payload,
                        signature,
                        _configuration.WebhookSecret,
                        _webhookSettings.SignatureToleranceSeconds,
                        throwOnApiVersionMismatch: false
                    );

                    _logger.LogDebug("Webhook signature validation passed for event {EventId}", stripeEvent.Id);
                    return Result<bool>.Success(true);
                }
                catch (StripeException stripeEx)
                {
                    _logger.LogWarning(stripeEx, "Webhook signature validation failed: {Message}", stripeEx.Message);
                    return Result.Failure<bool>(DomainErrors.Webhook.SignatureValidationFailed(stripeEx.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return Result.Failure<bool>(DomainErrors.Webhook.SignatureValidationFailed(ex.Message));
            }
        }

        /// <summary>
        /// Checks if webhook event is already processed (idempotency check)
        /// </summary>
        public async Task<Result<bool>> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(eventId))
                {
                    return Result.Failure<bool>(DomainErrors.Webhook.EventIdRequired);
                }

                var webhookLog = await _webhookLogRepository.GetByEventIdAsync(eventId, cancellationToken);
                var isProcessed = webhookLog != null &&
                                 (webhookLog.Status == WebhookStatus.Processed ||
                                  webhookLog.Status == WebhookStatus.Processing);

                if (isProcessed)
                {
                    _logger.LogDebug("Event {EventId} already processed (Status: {Status})",
                        eventId, webhookLog?.Status);
                }

                return Result<bool>.Success(isProcessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if event {EventId} is processed", eventId);
                return Result.Failure<bool>(DomainErrors.Webhook.EventStatusCheckFailed(ex.Message));
            }
        }

        /// <summary>
        /// Logs webhook as received (before processing)
        /// </summary>
        private async Task<Result> LogReceivedWebhookAsync(
            string eventId,
            string eventType,
            string payload,
            CancellationToken cancellationToken)
        {
            try
            {
                var existingLog = await _webhookLogRepository.GetByEventIdAsync(eventId, cancellationToken);
                if (existingLog != null)
                {
                    // Already logged, skip
                    return Result.Success();
                }

                var webhookLog = PaymentWebhookLog.CreateForWebhook(
                    eventId,
                    "Stripe",
                    eventType,
                    _webhookSettings.LogPayloads ? payload : "[Payload logging disabled]",
                    WebhookStatus.Processing);

                await _webhookLogRepository.AddAsync(webhookLog, cancellationToken);
                await _webhookLogRepository.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging received webhook {EventId}", eventId);
                // Don't fail webhook processing if logging fails
                return Result.Success();
            }
        }

        /// <summary>
        /// Marks webhook event as successfully processed
        /// </summary>
        public async Task<Result> MarkEventAsProcessedAsync(
            string eventId,
            string eventType,
            string payload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var webhookLog = await _webhookLogRepository.GetByEventIdAsync(eventId, cancellationToken);

                if (webhookLog == null)
                {
                    // Create new log if it doesn't exist
                    webhookLog = PaymentWebhookLog.CreateForWebhook(
                        eventId,
                        "Stripe",
                        eventType,
                        _webhookSettings.LogPayloads ? payload : "[Payload logging disabled]",
                        WebhookStatus.Processed);

                    await _webhookLogRepository.AddAsync(webhookLog, cancellationToken);
                }
                else
                {
                    // Update existing log
                    webhookLog.MarkAsProcessed();

                    await _webhookLogRepository.UpdateAsync(webhookLog, cancellationToken);
                }

                await _webhookLogRepository.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Marked webhook event {EventId} as processed", eventId);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                // Race condition - another process already marked it as processed
                _logger.LogInformation("Webhook event {EventId} already marked as processed by another process", eventId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking event {EventId} as processed", eventId);
                return Result.Failure(DomainErrors.General.UnexpectedError);
            }
        }

        /// <summary>
        /// Logs failed webhook processing
        /// </summary>
        private async Task<Result> LogFailedWebhookAsync(
            string? eventId,
            string eventType,
            string payload,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(eventId))
                {
                    eventId = $"unknown_{Guid.NewGuid()}";
                }

                var webhookLog = await _webhookLogRepository.GetByEventIdAsync(eventId, cancellationToken);

                if (webhookLog == null)
                {
                    webhookLog = PaymentWebhookLog.CreateForWebhook(
                        eventId,
                        "Stripe",
                        eventType,
                        _webhookSettings.LogPayloads ? TruncatePayload(payload) : "[Payload logging disabled]",
                        WebhookStatus.Failed);
                    webhookLog.MarkAsFailed(TruncateErrorMessage(errorMessage));

                    await _webhookLogRepository.AddAsync(webhookLog, cancellationToken);
                }
                else
                {
                    // Check if can retry before marking as failed
                    if (webhookLog.CanRetry(_webhookSettings.MaxRetryAttempts))
                    {
                        webhookLog.MarkAsFailed(TruncateErrorMessage(errorMessage));
                        _logger.LogInformation("Webhook {EventId} will be retried (Attempt {Attempts}/{MaxAttempts})",
                            eventId, webhookLog.Attempts, _webhookSettings.MaxRetryAttempts);
                    }
                    else
                    {
                        webhookLog.MarkAsFailed(TruncateErrorMessage(errorMessage));
                    }

                    await _webhookLogRepository.UpdateAsync(webhookLog, cancellationToken);
                }

                await _webhookLogRepository.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging failed webhook {EventId}", eventId);
                return Result.Failure(DomainErrors.General.UnexpectedError);
            }
        }

        /// <summary>
        /// Truncates payload for logging
        /// </summary>
        private string TruncatePayload(string payload, int maxLength = 2000)
        {
            if (string.IsNullOrEmpty(payload) || payload.Length <= maxLength)
            {
                return payload;
            }

            return payload.Substring(0, maxLength) + $"... [truncated, total length: {payload.Length}]";
        }

        /// <summary>
        /// Truncates error message for database storage
        /// </summary>
        private string TruncateErrorMessage(string errorMessage, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(errorMessage) || errorMessage.Length <= maxLength)
            {
                return errorMessage;
            }

            return errorMessage.Substring(0, maxLength) + "... [truncated]";
        }
    }

    /// <summary>
    /// Background job queue interface (implement based on your infrastructure)
    /// </summary>
    public interface IBackgroundJobQueue
    {
        void EnqueueWebhookProcessing(string eventId, string eventType, string payload);
    }
}