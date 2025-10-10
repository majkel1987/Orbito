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
                var originalBodyStream = context.Request.Body;
                Stream? memoryStream = null;
                
                try
                {
                    // Read the request body
                    memoryStream = new MemoryStream();
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

                        if (!IsValidStripeSignature(body, signature))
                        {
                            _logger.LogWarning("Invalid Stripe signature in webhook request");
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Invalid signature");
                            return;
                        }
                    }

                    // Restore the original body stream before calling next middleware
                    context.Request.Body = originalBodyStream;
                    
                    // Dispose memory stream after use
                    await memoryStream.DisposeAsync();
                    memoryStream = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing webhook signature verification");
                    
                    // Ensure original stream is restored even on exception
                    context.Request.Body = originalBodyStream;
                    
                    // Dispose memory stream if it was created
                    if (memoryStream != null)
                    {
                        await memoryStream.DisposeAsync();
                    }
                    
                    // Re-throw to let global exception handler deal with it
                    throw;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Validates Stripe webhook signature using HMAC-SHA256
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

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Stripe signature is empty");
                    return false;
                }

                // Parse signature header (format: t=timestamp,v1=signature)
                var signatureElements = signature.Split(',');
                string? timestamp = null;
                string? signatureHash = null;

                foreach (var element in signatureElements)
                {
                    var keyValue = element.Split('=', 2);
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "t":
                                timestamp = keyValue[1];
                                break;
                            case "v1":
                                signatureHash = keyValue[1];
                                break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signatureHash))
                {
                    _logger.LogWarning("Invalid signature format - missing timestamp or signature");
                    return false;
                }

                // Check timestamp tolerance (prevent replay attacks)
                if (!long.TryParse(timestamp, out var timestampLong))
                {
                    _logger.LogWarning("Invalid timestamp format in signature");
                    return false;
                }

                var eventTime = DateTimeOffset.FromUnixTimeSeconds(timestampLong);
                var currentTime = DateTimeOffset.UtcNow;
                var timeDifference = Math.Abs((currentTime - eventTime).TotalSeconds);

                if (timeDifference > _settings.SignatureToleranceSeconds)
                {
                    _logger.LogWarning("Signature timestamp is too old: {TimeDifference}s (tolerance: {Tolerance}s)", 
                        timeDifference, _settings.SignatureToleranceSeconds);
                    return false;
                }

                // Create expected signature
                var signedPayload = $"{timestamp}.{payload}";
                var expectedSignature = ComputeHmacSha256(signedPayload, _settings.WebhookSecret);

                // Compare signatures using constant-time comparison to prevent timing attacks
                var isValid = ConstantTimeEquals(signatureHash, expectedSignature);

                if (isValid)
                {
                    _logger.LogDebug("Stripe signature validation passed for timestamp {Timestamp}", timestamp);
                }
                else
                {
                    _logger.LogWarning("Stripe signature validation failed for timestamp {Timestamp}", timestamp);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Stripe signature");
                return false;
            }
        }

        /// <summary>
        /// Computes HMAC-SHA256 hash
        /// </summary>
        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// </summary>
        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            var result = 0;
            for (var i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
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
