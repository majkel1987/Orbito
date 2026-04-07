using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Helpers;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.BackgroundJobs;

/// <summary>
/// Background job that sends reminders for upcoming payments (3 days before due date)
/// </summary>
public class UpcomingPaymentReminderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpcomingPaymentReminderJob> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run daily
    private readonly TimeSpan _initialDelay;
    private const int DaysBeforePayment = 3;
    private const int OperationTimeoutMinutes = 15;

    public UpcomingPaymentReminderJob(
        IServiceProvider serviceProvider,
        ILogger<UpcomingPaymentReminderJob> logger)
        : this(serviceProvider, logger, TimeSpan.FromMinutes(1))
    {
    }

    public UpcomingPaymentReminderJob(
        IServiceProvider serviceProvider,
        ILogger<UpcomingPaymentReminderJob> logger,
        TimeSpan initialDelay)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialDelay = initialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpcomingPaymentReminderJob started");

        // Wait before first run to allow application to fully start
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendUpcomingPaymentRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending upcoming payment reminders");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("UpcomingPaymentReminderJob stopped");
    }

    private async Task SendUpcomingPaymentRemindersAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        var currentDate = dateTime.UtcNow;
        var reminderDate = currentDate.AddDays(DaysBeforePayment).Date;

        _logger.LogInformation("Processing upcoming payment reminders for date {ReminderDate}", reminderDate);

        // Create timeout for the operation
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // SECURE: Execute for all tenants with proper tenant context isolation
        var results = await TenantJobHelper.ExecuteForAllTenantsAsync(
            _serviceProvider,
            _logger,
            async (tenantId, serviceProvider, ct) =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var notificationService = serviceProvider.GetRequiredService<IPaymentNotificationService>();
                var dateTimeService = serviceProvider.GetRequiredService<IDateTime>();
                var reminderDateLocal = dateTimeService.UtcNow.AddDays(DaysBeforePayment).Date;

                // Get subscriptions for billing for THIS tenant only
                var subscriptions = await unitOfWork.Subscriptions.GetSubscriptionsForBillingAsync(
                    reminderDateLocal, ct);

                _logger.LogDebug("Tenant {TenantId}: Found {Count} subscriptions with upcoming payments on {Date}",
                    tenantId, subscriptions.Count(), reminderDateLocal);

                var successCount = 0;
                var failureCount = 0;

                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        // Send reminder notification
                        await notificationService.SendUpcomingPaymentReminderAsync(
                            subscription.Id,
                            DaysBeforePayment,
                            ct);

                        successCount++;
                        _logger.LogDebug("Sent upcoming payment reminder for subscription {SubscriptionId}",
                            subscription.Id);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex,
                            "Failed to send upcoming payment reminder for subscription {SubscriptionId}",
                            subscription.Id);
                    }

                    // Small delay to avoid overwhelming the email service
                    await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                }

                _logger.LogDebug(
                    "Completed upcoming payment reminders for tenant {TenantId}. Success: {SuccessCount}, Failed: {FailureCount}",
                    tenantId, successCount, failureCount);
            },
            linkedCts.Token);

        var successCount = results.Values.Count(r => r);
        _logger.LogInformation(
            "Completed upcoming payment reminder processing. Success: {SuccessCount}/{TotalCount}",
            successCount,
            results.Count);
    }
}
