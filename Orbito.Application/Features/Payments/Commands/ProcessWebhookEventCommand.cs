using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Commands.ProcessWebhookEvent
{
    /// <summary>
    /// Command for processing webhook events
    /// </summary>
    public record ProcessWebhookEventCommand : IRequest<Result>
    {
        /// <summary>
        /// Event type from webhook
        /// </summary>
        public required string EventType { get; init; }

        /// <summary>
        /// Event ID from webhook
        /// </summary>
        public required string EventId { get; init; }

        /// <summary>
        /// Raw webhook payload
        /// </summary>
        public required string Payload { get; init; }

        /// <summary>
        /// Webhook signature for verification
        /// </summary>
        public required string Signature { get; init; }

        /// <summary>
        /// Payment gateway provider
        /// </summary>
        public required string Provider { get; init; }
    }

    /// <summary>
    /// Handler for processing webhook events
    /// </summary>
    public class ProcessWebhookEventCommandHandler : IRequestHandler<ProcessWebhookEventCommand, Result>
    {
        private readonly IPaymentWebhookProcessor _webhookProcessor;
        private readonly ILogger<ProcessWebhookEventCommandHandler> _logger;

        public ProcessWebhookEventCommandHandler(
            IPaymentWebhookProcessor webhookProcessor,
            ILogger<ProcessWebhookEventCommandHandler> logger)
        {
            _webhookProcessor = webhookProcessor ?? throw new ArgumentNullException(nameof(webhookProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(ProcessWebhookEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing webhook event {EventType} from {Provider}", request.EventType, request.Provider);

                // Validate provider
                if (!IsValidProvider(request.Provider))
                {
                    _logger.LogWarning("Invalid provider {Provider} for webhook event {EventType}", request.Provider, request.EventType);
                    return Result.Failure("Invalid provider");
                }

                // Validate webhook signature
                var validationResult = await _webhookProcessor.ValidateWebhookSignatureAsync(request.Payload, request.Signature, cancellationToken);
                if (!validationResult.IsSuccess || !validationResult.Value)
                {
                    _logger.LogWarning("Invalid webhook signature for event {EventType} from {Provider}", request.EventType, request.Provider);
                    return Result.Failure("Invalid webhook signature");
                }

                // Check for idempotency
                var idempotencyCheck = await _webhookProcessor.IsEventProcessedAsync(request.EventId, cancellationToken);
                if (idempotencyCheck.IsSuccess && idempotencyCheck.Value)
                {
                    _logger.LogInformation("Webhook event {EventId} already processed, skipping", request.EventId);
                    return Result.Success();
                }

                // Process the webhook event
                var processResult = await _webhookProcessor.ProcessWebhookEventAsync(request.EventType, request.Payload, cancellationToken);
                if (!processResult.IsSuccess)
                {
                    _logger.LogError("Failed to process webhook event {EventId}: {Error}", request.EventId, processResult.ErrorMessage);
                    return processResult;
                }

                // Mark event as processed
                var markResult = await _webhookProcessor.MarkEventAsProcessedAsync(request.EventId, request.EventType, request.Payload, cancellationToken);
                if (!markResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to mark webhook event {EventId} as processed: {Error}", request.EventId, markResult.ErrorMessage);
                }

                _logger.LogInformation("Successfully processed webhook event {EventId} from {Provider}", request.EventId, request.Provider);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook event {EventType} from {Provider}", request.EventType, request.Provider);
                return Result.Failure($"Error processing webhook: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if the provider is supported
        /// </summary>
        private static bool IsValidProvider(string provider)
        {
            var validProviders = new[] { "Stripe", "PayPal", "Square" };
            return validProviders.Contains(provider, StringComparer.OrdinalIgnoreCase);
        }
    }
}
