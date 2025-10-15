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
        using var scope = _serviceProvider.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetService<ISubscriptionService>();
        var notificationService = scope.ServiceProvider.GetService<IPaymentNotificationService>();
        var dateTime = scope.ServiceProvider.GetService<IDateTime>();
        var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();

        if (subscriptionService == null || notificationService == null || dateTime == null || tenantContext == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        _logger.LogInformation("Checking for subscriptions expiring within {Days} days", DaysBeforeExpiry);

        try
        {
            // Set admin tenant context for background job
            // This allows access to all tenants' data for admin operations
            tenantContext.SetTenant(null); // Admin context - no tenant filtering

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Get subscriptions expiring in the next 7 days
            var expiringSubscriptions = await subscriptionService.GetExpiringSubscriptionsAsync(DaysBeforeExpiry, linkedCts.Token);

            _logger.LogInformation("Found {Count} subscriptions expiring within {Days} days",
                expiringSubscriptions.Count(), DaysBeforeExpiry);

            var successCount = 0;
            var failureCount = 0;

            foreach (var subscription in expiringSubscriptions)
            {
                try
                {
                    _logger.LogDebug("Subscription {SubscriptionId} for client {ClientId} is expiring on {ExpirationDate}",
                        subscription.Id, subscription.ClientId, subscription.NextBillingDate);

                    // Send expiration notification
                    await SendExpirationNotification(subscription, notificationService, dateTime, linkedCts.Token);

                    successCount++;
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex,
                        "Failed to send expiration notification for subscription {SubscriptionId}",
                        subscription.Id);
                }

                // Small delay to avoid overwhelming the notification service
                await Task.Delay(TimeSpan.FromMilliseconds(100), linkedCts.Token);
            }

            _logger.LogInformation(
                "Completed expiring subscription check. Success: {SuccessCount}, Failed: {FailureCount}",
                successCount, failureCount);
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
        finally
        {
            // Clear tenant context
            tenantContext.ClearTenant();
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
