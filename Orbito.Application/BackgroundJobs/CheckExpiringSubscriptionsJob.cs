using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Helpers;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.BackgroundJobs;

/// <summary>
/// Background job that checks for subscriptions expiring in the next 7 days and sends notifications
/// </summary>
public class CheckExpiringSubscriptionsJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CheckExpiringSubscriptionsJob> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run daily
    private readonly TimeSpan _initialDelay;
    private const int DaysBeforeExpiry = 7;
    private const int OperationTimeoutMinutes = 10;

    public CheckExpiringSubscriptionsJob(
        IServiceProvider serviceProvider,
        ILogger<CheckExpiringSubscriptionsJob> logger)
        : this(serviceProvider, logger, TimeSpan.FromMinutes(2))
    {
    }

    public CheckExpiringSubscriptionsJob(
        IServiceProvider serviceProvider,
        ILogger<CheckExpiringSubscriptionsJob> logger,
        TimeSpan initialDelay)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialDelay = initialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckExpiringSubscriptionsJob started");

        // Wait before first run to allow application to fully start
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckExpiringSubscriptions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expiring subscriptions");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("CheckExpiringSubscriptionsJob stopped");
    }

    private async Task CheckExpiringSubscriptions(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        _logger.LogInformation("Checking for subscriptions expiring within {Days} days", DaysBeforeExpiry);

        // Create timeout for the operation
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // SECURE: Execute for all tenants with proper tenant context isolation
        var results = await TenantJobHelper.ExecuteForAllTenantsAsync(
            _serviceProvider,
            _logger,
            async (tenantId, serviceProvider, ct) =>
            {
                var subscriptionRepository = serviceProvider.GetRequiredService<ISubscriptionRepository>();
                var notificationService = serviceProvider.GetRequiredService<IPaymentNotificationService>();
                var dateTimeService = serviceProvider.GetRequiredService<IDateTime>();
                var checkDate = dateTimeService.UtcNow;
                var tenantIdValueObject = TenantId.Create(tenantId);

                // SECURE: Get expiring subscriptions for THIS tenant only
                var expiringSubscriptions = await subscriptionRepository.GetExpiringSubscriptionsForTenantAsync(
                    tenantIdValueObject,
                    checkDate,
                    DaysBeforeExpiry,
                    ct);

                _logger.LogDebug("Tenant {TenantId}: Found {Count} expiring subscriptions",
                    tenantId, expiringSubscriptions.Count());

                var successCount = 0;
                var failureCount = 0;

                foreach (var subscription in expiringSubscriptions)
                {
                    try
                    {
                        _logger.LogDebug("Subscription {SubscriptionId} for client {ClientId} is expiring on {ExpirationDate}",
                            subscription.Id, subscription.ClientId, subscription.NextBillingDate);

                        // Calculate days until expiry
                        var daysUntilExpiry = (int)(subscription.NextBillingDate - checkDate).TotalDays;

                        // Send reminder notification for upcoming payment (subscription renewal)
                        await notificationService.SendUpcomingPaymentReminderAsync(
                            subscription.Id,
                            daysUntilExpiry,
                            ct);

                        successCount++;
                        _logger.LogDebug("Sent expiration notification for subscription {SubscriptionId}", subscription.Id);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex,
                            "Failed to send expiration notification for subscription {SubscriptionId}",
                            subscription.Id);
                    }

                    // Small delay to avoid overwhelming the notification service
                    await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                }

                _logger.LogDebug(
                    "Completed expiring subscription check for tenant {TenantId}. Success: {SuccessCount}, Failed: {FailureCount}",
                    tenantId, successCount, failureCount);
            },
            linkedCts.Token);

        var successCount = results.Values.Count(r => r);
        _logger.LogInformation(
            "Completed expiring subscription check. Success: {SuccessCount}/{TotalCount}",
            successCount,
            results.Count);
    }
}
