using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job for daily payment reconciliation with Stripe
/// Runs every day at 2:00 AM UTC
/// </summary>
public class DailyReconciliationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyReconciliationJob> _logger;
    private readonly ReconciliationSettings _settings;
    private readonly TimeSpan _targetTime;
    private const int ReconciliationWindowHours = 24;

    public DailyReconciliationJob(
        IServiceProvider serviceProvider,
        ILogger<DailyReconciliationJob> logger,
        IOptions<ReconciliationSettings> settings)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        // Parse DailyRunTime from configuration (format: "HH:mm")
        if (TimeSpan.TryParse(_settings.DailyRunTime, out var parsedTime))
        {
            _targetTime = parsedTime;
        }
        else
        {
            _logger.LogWarning(
                "Invalid DailyRunTime format '{DailyRunTime}' in configuration. Using default 02:00 UTC.",
                _settings.DailyRunTime);
            _targetTime = new TimeSpan(2, 0, 0); // Fallback to 2:00 AM UTC
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyReconciliationJob started. Will run daily at {TargetTime} UTC", _targetTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = CalculateNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation(
                    "Next reconciliation scheduled for {NextRun} UTC (in {DelayHours:F2} hours)",
                    nextRun, delay.TotalHours);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunReconciliationAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("DailyReconciliationJob cancelled");
                break;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "SECURITY: Unauthorized access in DailyReconciliationJob: {ErrorMessage}", ex.Message);
                // Wait 1 hour before retrying on security errors
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "CONFIGURATION: Invalid operation in DailyReconciliationJob: {ErrorMessage}", ex.Message);
                // Wait 30 minutes before retrying on configuration errors
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UNEXPECTED: Unexpected error in DailyReconciliationJob: {ErrorMessage}", ex.Message);

                // Wait 1 hour before retrying on unexpected errors
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("DailyReconciliationJob stopped");
    }

    /// <summary>
    /// Runs reconciliation for all active tenants
    /// </summary>
    private async Task RunReconciliationAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var reconciliationService = scope.ServiceProvider.GetRequiredService<IPaymentReconciliationService>();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

        var reconciliationStart = DateTime.UtcNow;
        _logger.LogInformation("Starting daily reconciliation at {StartTime} UTC", reconciliationStart);

        // TIMEOUT: Global timeout for entire reconciliation job (4 hours)
        using var globalCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        globalCts.CancelAfter(TimeSpan.FromHours(4));

        try
        {
            // Get all active tenants (ignore query filters for system-level job)
            var tenantIds = await context.Providers
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Select(p => p.TenantId.Value)
                .Distinct()
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Running reconciliation for {TenantCount} active tenants", tenantIds.Count);

            // Reconciliation period: last 24 hours
            var toDate = DateTime.UtcNow;
            var fromDate = toDate.AddHours(-ReconciliationWindowHours);

            int successCount = 0;
            int failureCount = 0;
            int totalDiscrepancies = 0;

            // Process each tenant sequentially to avoid overwhelming Stripe API
            foreach (var tenantIdGuid in tenantIds)
            {
                if (globalCts.Token.IsCancellationRequested)
                {
                    _logger.LogWarning("Reconciliation cancelled (stopping token or global timeout)");
                    break;
                }

                // SECURITY: Ensure tenant context is always cleaned up, even on timeout
                try
                {
                    var tenantId = TenantId.Create(tenantIdGuid);

                    // Set tenant context for this iteration
                    tenantProvider.SetTenantOverride(tenantIdGuid);

                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(globalCts.Token);
                    cts.CancelAfter(TimeSpan.FromMinutes(10)); // 10 min timeout per tenant

                    _logger.LogInformation(
                        "Starting reconciliation for tenant {TenantId} (period: {FromDate} to {ToDate})",
                        tenantId.Value, fromDate, toDate);

                    var report = await reconciliationService.ReconcileWithStripeAsync(
                        fromDate,
                        toDate,
                        tenantId,
                        cts.Token);

                    successCount++;
                    totalDiscrepancies += report.DiscrepanciesCount;

                    _logger.LogInformation(
                        "Reconciliation completed for tenant {TenantId}. " +
                        "Status: {Status}, Discrepancies: {DiscrepancyCount}, Auto-resolved: {AutoResolvedCount}, " +
                        "Manual review: {ManualReviewCount}",
                        tenantId.Value,
                        report.Status,
                        report.DiscrepanciesCount,
                        report.AutoResolvedCount,
                        report.ManualReviewCount);

                    // Send report notification
                    await reconciliationService.SendReconciliationReportAsync(report, cts.Token);
                }
                catch (OperationCanceledException ex) when (!stoppingToken.IsCancellationRequested)
                {
                    if (globalCts.Token.IsCancellationRequested)
                    {
                        _logger.LogError(
                            "Global timeout reached (4 hours). Aborting reconciliation for remaining tenants.");
                        failureCount++;
                        break; // Stop processing remaining tenants
                    }

                    _logger.LogWarning(ex,
                        "Reconciliation timed out for tenant {TenantId} after 10 minutes",
                        tenantIdGuid);
                    failureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to reconcile payments for tenant {TenantId}: {ErrorMessage}",
                        tenantIdGuid, ex.Message);
                    failureCount++;
                }
                finally
                {
                    // CRITICAL: Always clear tenant override to prevent context pollution
                    try
                    {
                        tenantProvider.ClearTenantOverride();
                        _logger.LogDebug("Tenant context cleared for {TenantId}", tenantIdGuid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "CRITICAL: Failed to clear tenant context for {TenantId}: {ErrorMessage}",
                            tenantIdGuid, ex.Message);
                    }
                }

                // Add small delay between tenants to avoid rate limiting
                if (!globalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), globalCts.Token);
                }
            }

            var reconciliationEnd = DateTime.UtcNow;
            var duration = reconciliationEnd - reconciliationStart;

            _logger.LogInformation(
                "Daily reconciliation completed. " +
                "Duration: {Duration:F2} minutes, " +
                "Tenants processed: {SuccessCount}/{TotalCount}, " +
                "Failures: {FailureCount}, " +
                "Total discrepancies found: {TotalDiscrepancies}",
                duration.TotalMinutes,
                successCount,
                tenantIds.Count,
                failureCount,
                totalDiscrepancies);

            // Log warning if there were failures
            if (failureCount > 0)
            {
                _logger.LogWarning(
                    "ATTENTION: {FailureCount} tenant reconciliations failed. Check logs for details.",
                    failureCount);
            }

            // Log critical alert if many discrepancies found
            if (totalDiscrepancies > 100)
            {
                _logger.LogCritical(
                    "CRITICAL: High number of discrepancies detected ({TotalDiscrepancies}). Immediate investigation required!",
                    totalDiscrepancies);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Critical error during daily reconciliation: {ErrorMessage}",
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Calculates next run time based on target time (2:00 AM UTC)
    /// </summary>
    private DateTime CalculateNextRunTime(DateTime now)
    {
        var today = now.Date;
        var todayTarget = today.Add(_targetTime);

        // If we've already passed today's target time, schedule for tomorrow
        if (now >= todayTarget)
        {
            return todayTarget.AddDays(1);
        }

        return todayTarget;
    }
}
