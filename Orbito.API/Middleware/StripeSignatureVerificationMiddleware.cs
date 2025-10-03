using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using System.Text;

namespace Orbito.API.Middleware
{
    /// <summary>
    /// Middleware for verifying Stripe webhook signatures
    /// </summary>
    public class StripeSignatureVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StripeSignatureVerificationMiddleware> _logger;
        private readonly StripeWebhookSettings _settings;

        public StripeSignatureVerificationMiddleware(
            RequestDelegate next,
            ILogger<StripeSignatureVerificationMiddleware> logger,
            IOptions<StripeWebhookSettings> settings)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply to webhook endpoints
            if (context.Request.Path.StartsWithSegments("/api/webhook"))
            {
                // Read the request body
                var originalBodyStream = context.Request.Body;
                using var memoryStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var body = await new StreamReader(memoryStream).ReadToEndAsync();
                memoryStream.Position = 0;

                // Replace the request body stream
                context.Request.Body = memoryStream;

                // Verify Stripe signature
                if (context.Request.Path.Value?.Contains("/stripe") == true)
                {
                    var signature = context.Request.Headers["Stripe-Signature"].FirstOrDefault();
                    if (string.IsNullOrEmpty(signature))
                    {
                        _logger.LogWarning("Missing Stripe signature in webhook request");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Missing signature");
                        return;
                    }

                    // TODO: Implement proper Stripe signature verification
                    // For now, we'll do basic validation
                    if (!IsValidStripeSignature(body, signature))
                    {
                        _logger.LogWarning("Invalid Stripe signature in webhook request");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Invalid signature");
                        return;
                    }
                }

                // Restore the original body stream
                context.Request.Body = originalBodyStream;
            }

            await _next(context);
        }

        /// <summary>
        /// Validates Stripe webhook signature
        /// </summary>
        /// <param name="payload">Request body</param>
        /// <param name="signature">Stripe signature header</param>
        /// <returns>True if signature is valid</returns>
        private bool IsValidStripeSignature(string payload, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.WebhookSecret))
                {
                    _logger.LogWarning("Stripe webhook secret not configured");
                    return false;
                }

                // TODO: Implement proper Stripe signature verification using HMAC-SHA256
                // This is a simplified version - in production, you should use Stripe's official verification method
                
                // Basic validation - check if signature contains expected elements
                if (!signature.Contains("t=") || !signature.Contains("v1="))
                {
                    _logger.LogWarning("Invalid signature format");
                    return false;
                }

                _logger.LogDebug("Stripe signature validation passed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Stripe signature");
                return false;
            }
        }
    }

    /// <summary>
    /// Extension method for registering the middleware
    /// </summary>
    public static class StripeSignatureVerificationMiddlewareExtensions
    {
        /// <summary>
        /// Adds Stripe signature verification middleware
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseStripeSignatureVerification(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StripeSignatureVerificationMiddleware>();
        }
    }
}
