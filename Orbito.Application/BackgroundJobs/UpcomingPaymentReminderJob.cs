using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetService<IPaymentNotificationService>();
        var dateTime = scope.ServiceProvider.GetService<IDateTime>();
        var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();

        if (unitOfWork == null || notificationService == null || dateTime == null || tenantContext == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        var currentDate = dateTime.UtcNow;
        var reminderDate = currentDate.AddDays(DaysBeforePayment).Date;

        _logger.LogInformation("Processing upcoming payment reminders for date {ReminderDate}", reminderDate);

        try
        {
            // Set admin tenant context for background job
            // This allows access to all tenants' data for admin operations
            tenantContext.SetTenant(null); // Admin context - no tenant filtering

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Get all active subscriptions with billing dates matching the reminder date
            var subscriptions = await unitOfWork.Subscriptions.GetSubscriptionsForBillingAsync(
                reminderDate, linkedCts.Token);

            _logger.LogInformation("Found {Count} subscriptions with upcoming payments on {Date}",
                subscriptions.Count(), reminderDate);

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
                        linkedCts.Token);

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
                await Task.Delay(TimeSpan.FromMilliseconds(100), linkedCts.Token);
            }

            _logger.LogInformation(
                "Completed upcoming payment reminder processing. Success: {SuccessCount}, Failed: {FailureCount}",
                successCount, failureCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("SendUpcomingPaymentReminders operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("SendUpcomingPaymentReminders operation timed out after {Minutes} minutes", OperationTimeoutMinutes);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing upcoming payment reminders");
            throw;
        }
        finally
        {
            // Clear tenant context
            tenantContext.ClearTenant();
            
            // Properly dispose UnitOfWork
            if (unitOfWork is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}
