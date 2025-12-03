using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Helpers;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.BackgroundJobs;

/// <summary>
/// Background job that processes recurring payments and expired subscriptions hourly
/// </summary>
public class ProcessRecurringPaymentsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessRecurringPaymentsJob> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run hourly
    private readonly TimeSpan _initialDelay;
    private const int OperationTimeoutMinutes = 15;

    public ProcessRecurringPaymentsJob(
        IServiceProvider serviceProvider,
        ILogger<ProcessRecurringPaymentsJob> logger)
        : this(serviceProvider, logger, TimeSpan.FromMinutes(3))
    {
    }

    public ProcessRecurringPaymentsJob(
        IServiceProvider serviceProvider,
        ILogger<ProcessRecurringPaymentsJob> logger,
        TimeSpan initialDelay)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialDelay = initialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProcessRecurringPaymentsJob started");

        // Wait before first run to allow application to fully start
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRecurringPayments(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing recurring payments");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("ProcessRecurringPaymentsJob stopped");
    }

    private async Task ProcessRecurringPayments(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dateTime = scope.ServiceProvider.GetService<IDateTime>();

        if (dateTime == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        var currentDate = dateTime.UtcNow;

        _logger.LogInformation("Processing recurring payments for date {Date}", currentDate.Date);

        // Create timeout for the operation
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Execute for all tenants
        var results = await TenantJobHelper.ExecuteForAllTenantsAsync(
            _serviceProvider,
            _logger,
            async (tenantId, serviceProvider, ct) =>
            {
                var subscriptionService = serviceProvider.GetRequiredService<ISubscriptionService>();
                var dateTimeService = serviceProvider.GetRequiredService<IDateTime>();
                var currentDateLocal = dateTimeService.UtcNow;

                // Process recurring payments for today
                try
                {
                    await subscriptionService.ProcessRecurringPaymentsAsync(currentDateLocal, ct);
                    _logger.LogDebug("Successfully processed recurring payments for tenant {TenantId}", tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process recurring payments for tenant {TenantId}", tenantId);
                    throw;
                }

                // Process expired subscriptions
                try
                {
                    await subscriptionService.ProcessExpiredSubscriptionsAsync(ct);
                    _logger.LogDebug("Successfully processed expired subscriptions for tenant {TenantId}", tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process expired subscriptions for tenant {TenantId}", tenantId);
                    throw;
                }
            },
            linkedCts.Token);

        var successCount = results.Values.Count(r => r);
        _logger.LogInformation(
            "Completed recurring payments job. Success: {SuccessCount}/{TotalCount}",
            successCount,
            results.Count);
    }
}
