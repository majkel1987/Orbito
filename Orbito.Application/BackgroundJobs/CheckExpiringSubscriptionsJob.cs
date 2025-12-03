using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

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
        var subscriptionRepository = scope.ServiceProvider.GetService<ISubscriptionRepository>();
        var providerRepository = scope.ServiceProvider.GetService<IProviderRepository>();
        var notificationService = scope.ServiceProvider.GetService<IPaymentNotificationService>();
        var dateTime = scope.ServiceProvider.GetService<IDateTime>();

        if (subscriptionRepository == null || providerRepository == null || notificationService == null || dateTime == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        _logger.LogInformation("Checking for subscriptions expiring within {Days} days", DaysBeforeExpiry);

        try
        {
            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Get all active providers to iterate through tenants
            var providers = await providerRepository.GetActiveProvidersAsync(1, int.MaxValue, linkedCts.Token);
            var tenantIds = providers.Select(p => p.TenantId).Distinct().ToList();

            _logger.LogDebug("Found {Count} active tenants to check", tenantIds.Count);

            var totalSuccessCount = 0;
            var totalFailureCount = 0;

            // Process each tenant separately with explicit TenantId
            foreach (var tenantId in tenantIds)
            {
                try
                {
                    var checkDate = dateTime.UtcNow;

                    // SECURE: Explicitly pass TenantId to prevent cross-tenant access
                    var expiringSubscriptions = await subscriptionRepository.GetExpiringSubscriptionsForTenantAsync(
                        tenantId,
                        checkDate,
                        DaysBeforeExpiry,
                        linkedCts.Token);

                    _logger.LogDebug("Tenant {TenantId}: Found {Count} expiring subscriptions",
                        tenantId.Value, expiringSubscriptions.Count());

                    foreach (var subscription in expiringSubscriptions)
                    {
                        try
                        {
                            _logger.LogDebug("Subscription {SubscriptionId} for client {ClientId} is expiring on {ExpirationDate}",
                                subscription.Id, subscription.ClientId, subscription.NextBillingDate);

                            // Send expiration notification
                            await SendExpirationNotification(subscription, notificationService, dateTime, linkedCts.Token);

                            totalSuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            totalFailureCount++;
                            _logger.LogError(ex,
                                "Failed to send expiration notification for subscription {SubscriptionId}",
                                subscription.Id);
                        }

                        // Small delay to avoid overwhelming the notification service
                        await Task.Delay(TimeSpan.FromMilliseconds(100), linkedCts.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tenant {TenantId}", tenantId.Value);
                    // Continue with next tenant
                }
            }

            _logger.LogInformation(
                "Completed expiring subscription check. Success: {SuccessCount}, Failed: {FailureCount}",
                totalSuccessCount, totalFailureCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("CheckExpiringSubscriptions operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("CheckExpiringSubscriptions operation timed out after {Minutes} minutes", OperationTimeoutMinutes);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking expiring subscriptions");
            throw;
        }
    }

    private async Task SendExpirationNotification(
        Domain.Entities.Subscription subscription,
        IPaymentNotificationService notificationService,
        IDateTime dateTime,
        CancellationToken cancellationToken)
    {
        // Calculate days until expiry
        var daysUntilExpiry = (int)(subscription.NextBillingDate - dateTime.UtcNow).TotalDays;

        // Send reminder notification for upcoming payment (subscription renewal)
        await notificationService.SendUpcomingPaymentReminderAsync(
            subscription.Id,
            daysUntilExpiry,
            cancellationToken);

        _logger.LogDebug("Sent expiration notification for subscription {SubscriptionId}", subscription.Id);
    }
}
