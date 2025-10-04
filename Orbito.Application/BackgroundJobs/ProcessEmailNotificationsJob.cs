using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.BackgroundJobs;

/// <summary>
/// Background job for processing email notifications from outbox
/// </summary>
public class ProcessEmailNotificationsJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessEmailNotificationsJob> _logger;

    public ProcessEmailNotificationsJob(
        IServiceProvider serviceProvider,
        ILogger<ProcessEmailNotificationsJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Processes pending email notifications with retry mechanism
    /// </summary>
    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting email notification processing");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            // Get pending notifications that are ready for retry
            var pendingNotifications = await unitOfWork.EmailNotifications
                .GetPendingNotificationsAsync(cancellationToken);

            _logger.LogInformation("Found {Count} pending email notifications", pendingNotifications.Count);

            var processedCount = 0;
            var failedCount = 0;

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    // Check if notification can be retried
                    if (!notification.CanRetry())
                    {
                        _logger.LogDebug("Notification {NotificationId} not ready for retry yet", notification.Id);
                        continue;
                    }

                    _logger.LogInformation("Processing email notification {NotificationId} (attempt {RetryCount}/{MaxRetries})",
                        notification.Id, notification.RetryCount + 1, notification.MaxRetries);

                    // Try to send email
                    await emailSender.SendEmailAsync(
                        notification.RecipientEmail,
                        notification.Subject,
                        notification.Body,
                        isHtml: false,
                        cancellationToken);

                    // Mark as processed
                    notification.MarkAsProcessed();
                    await unitOfWork.EmailNotifications.UpdateAsync(notification, cancellationToken);
                    processedCount++;

                    _logger.LogInformation("Email notification {NotificationId} sent successfully", notification.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email notification {NotificationId}", notification.Id);

                    // Schedule retry with exponential backoff
                    notification.ScheduleRetry();
                    await unitOfWork.EmailNotifications.UpdateAsync(notification, cancellationToken);
                    failedCount++;

                    _logger.LogInformation("Email notification {NotificationId} scheduled for retry in {NextRetryAt}",
                        notification.Id, notification.NextRetryAt);
                }
            }

            // Save all changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email notification processing completed. Processed: {ProcessedCount}, Failed: {FailedCount}",
                processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email notifications");
        }
    }

    /// <summary>
    /// Processes failed notifications for final cleanup
    /// </summary>
    public async Task CleanupFailedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting cleanup of failed email notifications");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Get notifications that have exceeded max retries
            var failedNotifications = await unitOfWork.EmailNotifications
                .GetFailedNotificationsAsync(cancellationToken);

            _logger.LogInformation("Found {Count} failed email notifications for cleanup", failedNotifications.Count);

            foreach (var notification in failedNotifications)
            {
                _logger.LogWarning("Email notification {NotificationId} failed permanently after {RetryCount} attempts. Error: {ErrorMessage}",
                    notification.Id, notification.RetryCount, notification.ErrorMessage);
            }

            _logger.LogInformation("Failed email notifications cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up failed email notifications");
        }
    }
}
