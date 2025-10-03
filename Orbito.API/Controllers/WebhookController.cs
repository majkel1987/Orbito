using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using MediatR;
using Orbito.Application.Features.Payments.Commands.ProcessWebhookEvent;
using System.Text;
using System.Text.Json;

namespace Orbito.API.Controllers
{
    /// <summary>
    /// Controller for handling webhook events from payment gateways
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IMediator _mediator;

        public WebhookController(
            ILogger<WebhookController> logger,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Handles Stripe webhook events
        /// </summary>
        /// <returns>Webhook processing result</returns>
        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            try
            {
                // Read the raw request body
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var payload = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(payload))
                {
                    _logger.LogWarning("Received empty Stripe webhook payload");
                    return BadRequest(new { error = "Empty payload" });
                }

                // Get the Stripe signature from headers
                var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Missing Stripe signature in webhook request");
                    return BadRequest(new { error = "Missing signature" });
                }

                // Parse Stripe event to get ID and type
                var stripeEvent = ParseStripeEvent(payload);
                if (stripeEvent == null)
                {
                    _logger.LogWarning("Invalid Stripe webhook payload format");
                    return BadRequest(new { error = "Invalid payload format" });
                }

                _logger.LogInformation("Received Stripe webhook: {EventType}, EventId: {EventId}",
                    stripeEvent.Type, stripeEvent.Id);

                // Create command with EventId for idempotency
                var command = new ProcessWebhookEventCommand
                {
                    EventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    Payload = payload,
                    Signature = signature,
                    Provider = "Stripe"
                };

                // Process webhook via MediatR (includes validation)
                var result = await _mediator.Send(command);

                // Webhook best practice: return 200 for known errors to prevent retries
                if (!result.IsSuccess)
                {
                    if (IsKnownError(result.ErrorMessage))
                    {
                        _logger.LogWarning("Known error processing webhook {EventId}: {Error}",
                            stripeEvent.Id, result.ErrorMessage);
                        return Ok(new { received = true, processed = false, reason = "already_processed_or_known_error" });
                    }

                    // 500 only for unexpected errors
                    _logger.LogError("Unexpected error processing webhook {EventId}: {Error}",
                        stripeEvent.Id, result.ErrorMessage);
                    return StatusCode(500, new { error = "Unexpected error" });
                }

                _logger.LogInformation("Successfully processed Stripe webhook: {EventType}, EventId: {EventId}",
                    stripeEvent.Type, stripeEvent.Id);
                return Ok(new { received = true, processed = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Parses Stripe event payload to extract event ID and type
        /// </summary>
        private StripeEventInfo? ParseStripeEvent(string payload)
        {
            try
            {
                using var json = JsonDocument.Parse(payload);
                var root = json.RootElement;

                if (!root.TryGetProperty("id", out var idElement) ||
                    !root.TryGetProperty("type", out var typeElement))
                {
                    return null;
                }

                return new StripeEventInfo
                {
                    Id = idElement.GetString() ?? string.Empty,
                    Type = typeElement.GetString() ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if error is known and should not trigger retries
        /// </summary>
        private bool IsKnownError(string? errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return false;
            }

            var knownErrors = new[]
            {
                "already processed",
                "duplicate event",
                "invalid webhook signature",
                "unknown event type",
                "event already exists"
            };

            return knownErrors.Any(e => errorMessage.Contains(e, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Stripe event info for idempotency
        /// </summary>
        private class StripeEventInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }

        /// <summary>
        /// Handles generic webhook events (for future payment gateways)
        /// </summary>
        /// <param name="provider">Payment gateway provider</param>
        /// <returns>Webhook processing result</returns>
        [HttpPost("{provider}")]
        public async Task<IActionResult> HandleWebhook(string provider)
        {
            // For now, redirect to Stripe endpoint
            if (provider.Equals("stripe", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleStripeWebhook();
            }

            _logger.LogWarning("Unsupported webhook provider: {Provider}", provider);
            return BadRequest(new { error = $"Unsupported provider: {provider}" });
        }

        /// <summary>
        /// Health check endpoint for webhook processing
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
