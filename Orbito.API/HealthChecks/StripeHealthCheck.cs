using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Infrastructure.PaymentGateways.Stripe;
using Stripe;
using System.Diagnostics;

namespace Orbito.API.HealthChecks;

/// <summary>
/// Health check for Stripe API connectivity
/// Checks Stripe API availability by calling GET /v1/balance endpoint
/// </summary>
public class StripeHealthCheck : IHealthCheck
{
    private readonly Orbito.Infrastructure.PaymentGateways.Stripe.StripeConfiguration _stripeConfig;
    private readonly ILogger<StripeHealthCheck> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public StripeHealthCheck(
        IOptions<Orbito.Infrastructure.PaymentGateways.Stripe.StripeConfiguration> stripeConfig,
        ILogger<StripeHealthCheck> logger)
    {
        _stripeConfig = stripeConfig.Value ?? throw new ArgumentNullException(nameof(stripeConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting Stripe health check");

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_stripeConfig.SecretKey))
            {
                return HealthCheckResult.Unhealthy(
                    "Stripe configuration is missing SecretKey",
                    data: new Dictionary<string, object>
                    {
                        ["error"] = "Missing SecretKey",
                        ["timestamp"] = DateTime.UtcNow
                    });
            }

            // Check if we're in test mode and warn
            if (_stripeConfig.IsTestEnvironment())
            {
                _logger.LogDebug("Stripe health check running in TEST mode");
            }

            // Create Stripe service with timeout
            var balanceService = new BalanceService();
            
            // Set API key for this request
            var requestOptions = new RequestOptions
            {
                ApiKey = _stripeConfig.SecretKey
            };

            // Use Stopwatch to measure response time
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Call Stripe balance endpoint (lightweight API call)
                var balance = await balanceService.GetAsync(requestOptions, cancellationToken);
                stopwatch.Stop();

                var responseTime = stopwatch.ElapsedMilliseconds;
                var isHealthy = responseTime <= 3000; // 3 seconds threshold for healthy
                var isDegraded = responseTime > 3000 && responseTime <= 5000; // 3-5 seconds is degraded

                var healthData = new Dictionary<string, object>
                {
                    ["response_time_ms"] = responseTime,
                    ["environment"] = _stripeConfig.Environment,
                    ["timestamp"] = DateTime.UtcNow,
                    ["available_currencies"] = balance.Available?.Count ?? 0
                };

                if (isHealthy)
                {
                    _logger.LogDebug("Stripe health check passed in {ResponseTime}ms", responseTime);
                    return HealthCheckResult.Healthy(
                        $"Stripe API is healthy (response time: {responseTime}ms)",
                        healthData);
                }
                else if (isDegraded)
                {
                    _logger.LogWarning("Stripe health check degraded - slow response time: {ResponseTime}ms", responseTime);
                    return HealthCheckResult.Degraded(
                        $"Stripe API is degraded (response time: {responseTime}ms)",
                        data: healthData);
                }
                else
                {
                    _logger.LogError("Stripe health check failed - timeout exceeded: {ResponseTime}ms", responseTime);
                    return HealthCheckResult.Unhealthy(
                        $"Stripe API is unhealthy (response time: {responseTime}ms)",
                        data: healthData);
                }
            }
            catch (StripeException stripeEx)
            {
                stopwatch.Stop();
                _logger.LogError(stripeEx, "Stripe API error during health check: {Error}", stripeEx.Message);

                return HealthCheckResult.Unhealthy(
                    $"Stripe API error: {stripeEx.Message}",
                    stripeEx,
                    new Dictionary<string, object>
                    {
                        ["error"] = stripeEx.Message,
                        ["stripe_error_type"] = stripeEx.StripeError?.Type ?? "unknown",
                        ["stripe_error_code"] = stripeEx.StripeError?.Code ?? "unknown",
                        ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                        ["timestamp"] = DateTime.UtcNow
                    });
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Stripe health check timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            return HealthCheckResult.Unhealthy(
                $"Stripe API timeout after {_timeout.TotalMilliseconds}ms",
                data: new Dictionary<string, object>
                {
                    ["error"] = "Timeout",
                    ["timeout_ms"] = _timeout.TotalMilliseconds,
                    ["timestamp"] = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Stripe health check");
            return HealthCheckResult.Unhealthy(
                $"Unexpected error: {ex.Message}",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["error_type"] = ex.GetType().Name,
                    ["timestamp"] = DateTime.UtcNow
                });
        }
    }
}
