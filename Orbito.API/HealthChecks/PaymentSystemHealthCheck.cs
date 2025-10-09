using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;
using Orbito.Infrastructure.PaymentGateways.Stripe.Models;
using System.Diagnostics;

namespace Orbito.API.HealthChecks;

/// <summary>
/// Composite health check for the payment system
/// Checks database connectivity, failed payments ratio, pending retries count, and webhook response time
/// </summary>
public class PaymentSystemHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentSystemHealthCheck> _logger;
    private readonly MonitoringSettings _monitoringSettings;

    public PaymentSystemHealthCheck(
        ApplicationDbContext context,
        ILogger<PaymentSystemHealthCheck> logger,
        IOptions<MonitoringSettings> monitoringSettings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _monitoringSettings = monitoringSettings?.Value ?? throw new ArgumentNullException(nameof(monitoringSettings));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow
        };

        var issues = new List<string>();
        var warnings = new List<string>();

        try
        {
            _logger.LogDebug("Starting payment system health check");

            // 1. Check database connectivity
            var dbHealthResult = await CheckDatabaseConnectivityAsync(cancellationToken);
            healthData["database"] = dbHealthResult;
            
            if (dbHealthResult["status"]?.ToString() != "healthy")
            {
                issues.Add($"Database: {dbHealthResult["error"]}");
            }

            // 2. Check failed payments ratio (last 1 hour)
            var failedPaymentsResult = await CheckFailedPaymentsRatioAsync(cancellationToken);
            healthData["failed_payments_ratio"] = failedPaymentsResult;
            
            var failureRate = Convert.ToDouble(failedPaymentsResult["failure_rate_percent"]);
            if (failureRate > _monitoringSettings.FailureRateThresholdPercent)
            {
                warnings.Add($"High failure rate: {failureRate:F1}% (threshold: {_monitoringSettings.FailureRateThresholdPercent}%)");
            }

            // 3. Check pending retries count
            var pendingRetriesResult = await CheckPendingRetriesCountAsync(cancellationToken);
            healthData["pending_retries"] = pendingRetriesResult;
            
            var pendingCount = Convert.ToInt32(pendingRetriesResult["count"]);
            if (pendingCount > _monitoringSettings.MaxPendingRetries)
            {
                warnings.Add($"High pending retries count: {pendingCount} (threshold: {_monitoringSettings.MaxPendingRetries})");
            }

            // 4. Check webhook response time (simulated - in real implementation would check actual webhook logs)
            var webhookHealthResult = await CheckWebhookResponseTimeAsync(cancellationToken);
            healthData["webhook_response_time"] = webhookHealthResult;
            
            var avgResponseTime = Convert.ToDouble(webhookHealthResult["avg_response_time_ms"]);
            if (avgResponseTime > 3000) // 3 seconds threshold
            {
                warnings.Add($"Slow webhook response time: {avgResponseTime:F0}ms (threshold: 3000ms)");
            }

            // Determine overall health status
            var hasIssues = issues.Any();
            var hasWarnings = warnings.Any();

            if (hasIssues)
            {
                _logger.LogError("Payment system health check failed with issues: {Issues}", string.Join("; ", issues));
                return HealthCheckResult.Unhealthy(
                    $"Payment system unhealthy: {string.Join("; ", issues)}",
                    data: healthData);
            }
            else if (hasWarnings)
            {
                _logger.LogWarning("Payment system health check degraded with warnings: {Warnings}", string.Join("; ", warnings));
                return HealthCheckResult.Degraded(
                    $"Payment system degraded: {string.Join("; ", warnings)}",
                    data: healthData);
            }
            else
            {
                _logger.LogDebug("Payment system health check passed");
                return HealthCheckResult.Healthy(
                    "Payment system is healthy",
                    healthData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during payment system health check");
            return HealthCheckResult.Unhealthy(
                $"Payment system health check failed: {ex.Message}",
                ex,
                healthData);
        }
    }

    private async Task<Dictionary<string, object>> CheckDatabaseConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Simple database connectivity check
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            if (canConnect)
            {
                return new Dictionary<string, object>
                {
                    ["status"] = "healthy",
                    ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                    ["database"] = "connected"
                };
            }
            else
            {
                return new Dictionary<string, object>
                {
                    ["status"] = "unhealthy",
                    ["error"] = "Cannot connect to database",
                    ["response_time_ms"] = stopwatch.ElapsedMilliseconds
                };
            }
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                ["status"] = "unhealthy",
                ["error"] = ex.Message,
                ["exception_type"] = ex.GetType().Name
            };
        }
    }

    private async Task<Dictionary<string, object>> CheckFailedPaymentsRatioAsync(CancellationToken cancellationToken)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            // Count total payments in last hour
            var totalPayments = await _context.Payments
                .Where(p => p.CreatedAt >= oneHourAgo)
                .CountAsync(cancellationToken);

            // Count failed payments in last hour
            var failedPayments = await _context.Payments
                .Where(p => p.CreatedAt >= oneHourAgo && p.Status == PaymentStatus.Failed)
                .CountAsync(cancellationToken);

            var failureRate = totalPayments > 0 ? (double)failedPayments / totalPayments * 100 : 0;

            return new Dictionary<string, object>
            {
                ["total_payments"] = totalPayments,
                ["failed_payments"] = failedPayments,
                ["failure_rate_percent"] = Math.Round(failureRate, 2),
                ["threshold_percent"] = _monitoringSettings.FailureRateThresholdPercent,
                ["time_window_hours"] = 1
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["failure_rate_percent"] = 0
            };
        }
    }

    private async Task<Dictionary<string, object>> CheckPendingRetriesCountAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Count pending retry schedules
            var pendingRetries = await _context.PaymentRetrySchedules
                .Where(prs => prs.Status == RetryStatus.Scheduled || prs.Status == RetryStatus.InProgress)
                .CountAsync(cancellationToken);

            return new Dictionary<string, object>
            {
                ["count"] = pendingRetries,
                ["threshold"] = _monitoringSettings.MaxPendingRetries,
                ["status"] = pendingRetries <= _monitoringSettings.MaxPendingRetries ? "healthy" : "degraded"
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["count"] = 0
            };
        }
    }

    private async Task<Dictionary<string, object>> CheckWebhookResponseTimeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            // Get average webhook processing time from logs
            // Since PaymentWebhookLog doesn't have ProcessingTimeMs, we'll calculate it from timestamps
            var webhookLogs = await _context.PaymentWebhookLogs
                .Where(wl => wl.ReceivedAt >= oneHourAgo && wl.ProcessedAt.HasValue)
                .Select(wl => (wl.ProcessedAt!.Value - wl.ReceivedAt).TotalMilliseconds)
                .ToListAsync(cancellationToken);

            var avgResponseTime = webhookLogs.Any() ? webhookLogs.Average() : 0;
            var maxResponseTime = webhookLogs.Any() ? webhookLogs.Max() : 0;

            return new Dictionary<string, object>
            {
                ["avg_response_time_ms"] = Math.Round(avgResponseTime, 2),
                ["max_response_time_ms"] = maxResponseTime,
                ["total_webhooks"] = webhookLogs.Count,
                ["threshold_ms"] = 3000,
                ["status"] = avgResponseTime <= 3000 ? "healthy" : "degraded"
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["avg_response_time_ms"] = 0
            };
        }
    }
}

/// <summary>
/// Configuration settings for monitoring thresholds
/// </summary>
public class MonitoringSettings
{
    /// <summary>
    /// Threshold for failed payments ratio (percentage)
    /// </summary>
    public int FailureRateThresholdPercent { get; set; } = 20;

    /// <summary>
    /// Maximum number of pending retries before system is considered degraded
    /// </summary>
    public int MaxPendingRetries { get; set; } = 1000;

    /// <summary>
    /// Timeout for Stripe health check in seconds
    /// </summary>
    public int StripeHealthCheckTimeoutSeconds { get; set; } = 5;
}
