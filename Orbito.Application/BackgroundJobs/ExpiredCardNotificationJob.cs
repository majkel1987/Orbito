using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.BackgroundJobs;

/// <summary>
/// Background job that sends notifications for expired and expiring payment cards
/// </summary>
public class ExpiredCardNotificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredCardNotificationJob> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run daily
    private readonly TimeSpan _initialDelay;
    private const int DaysBeforeExpiryToNotify = 30; // Notify 30 days before expiry
    private const int OperationTimeoutMinutes = 20;

    public ExpiredCardNotificationJob(
        IServiceProvider serviceProvider,
        ILogger<ExpiredCardNotificationJob> logger)
        : this(serviceProvider, logger, TimeSpan.FromMinutes(5))
    {
    }

    public ExpiredCardNotificationJob(
        IServiceProvider serviceProvider,
        ILogger<ExpiredCardNotificationJob> logger,
        TimeSpan initialDelay)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _initialDelay = initialDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredCardNotificationJob started");

        // Wait before first run to allow application to fully start
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredCardsAsync(stoppingToken);
                await ProcessExpiringCardsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired/expiring cards");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("ExpiredCardNotificationJob stopped");
    }

    /// <summary>
    /// Processes and sends notifications for expired cards
    /// </summary>
    private async Task ProcessExpiredCardsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetService<IPaymentNotificationService>();
        var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();

        if (unitOfWork == null || notificationService == null || tenantContext == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        _logger.LogInformation("Processing expired payment cards");

        try
        {
            // Set admin tenant context for background job
            // This allows access to all tenants' data for admin operations
            tenantContext.SetTenant(null); // Admin context - no tenant filtering

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Get all expired payment methods
            var expiredPaymentMethods = await unitOfWork.PaymentMethods.GetExpiredPaymentMethodsAsync(linkedCts.Token);

            _logger.LogInformation("Found {Count} expired payment methods", expiredPaymentMethods.Count());

            var successCount = 0;
            var failureCount = 0;

            foreach (var paymentMethod in expiredPaymentMethods)
            {
                try
                {
                    // Send expired card notification
                    await notificationService.SendExpiredCardNotificationAsync(
                        paymentMethod.Id,
                        linkedCts.Token);

                    successCount++;
                    _logger.LogDebug("Sent expired card notification for payment method {PaymentMethodId}",
                        paymentMethod.Id);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex,
                        "Failed to send expired card notification for payment method {PaymentMethodId}",
                        paymentMethod.Id);
                }

                // Small delay to avoid overwhelming the email service
                await Task.Delay(TimeSpan.FromMilliseconds(100), linkedCts.Token);
            }

            _logger.LogInformation(
                "Completed expired card notification processing. Success: {SuccessCount}, Failed: {FailureCount}",
                successCount, failureCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ProcessExpiredCards operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("ProcessExpiredCards operation timed out after {Minutes} minutes", OperationTimeoutMinutes);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired payment cards");
            throw;
        }
        finally
        {
            // Clear tenant context
            tenantContext.ClearTenant();
        }
    }

    /// <summary>
    /// Processes and sends notifications for cards expiring soon (within 30 days)
    /// </summary>
    private async Task ProcessExpiringCardsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

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
        var expiryThresholdDate = currentDate.AddDays(DaysBeforeExpiryToNotify);

        _logger.LogInformation("Processing payment cards expiring before {ThresholdDate}", expiryThresholdDate.Date);

        try
        {
            // Set admin tenant context for background job
            // This allows access to all tenants' data for admin operations
            tenantContext.SetTenant(null); // Admin context - no tenant filtering

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var successCount = 0;
            var failureCount = 0;
            var pageNumber = 1;
            const int pageSize = 100; // Process in batches of 100
            var hasMore = true;

            // Process all cards in batches to avoid memory issues with large datasets
            while (hasMore)
            {
                var paymentMethods = await unitOfWork.PaymentMethods.GetByTypeAsync(
                    Domain.Enums.PaymentMethodType.Card,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    cancellationToken: linkedCts.Token);

                var paymentMethodsList = paymentMethods.ToList();
                hasMore = paymentMethodsList.Count == pageSize;

                // Filter for cards expiring within the threshold
                var expiringPaymentMethods = paymentMethodsList
                    .Where(pm => pm.ExpiryDate.HasValue &&
                                 !pm.IsExpired() &&
                                 pm.ExpiryDate.Value.Date <= expiryThresholdDate.Date)
                    .ToList();

                _logger.LogDebug("Processing page {Page}: found {Count} expiring cards out of {Total} cards in batch",
                    pageNumber, expiringPaymentMethods.Count, paymentMethodsList.Count);

                foreach (var paymentMethod in expiringPaymentMethods)
                {
                    try
                    {
                        // Calculate days until expiry
                        var daysUntilExpiry = (int)(paymentMethod.ExpiryDate!.Value.Date - currentDate.Date).TotalDays;

                        // Send card expiring soon notification
                        await notificationService.SendCardExpiringSoonNotificationAsync(
                            paymentMethod.Id,
                            daysUntilExpiry,
                            linkedCts.Token);

                        successCount++;
                        _logger.LogDebug(
                            "Sent expiring card notification for payment method {PaymentMethodId} (expires in {Days} days)",
                            paymentMethod.Id, daysUntilExpiry);
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex,
                            "Failed to send expiring card notification for payment method {PaymentMethodId}",
                            paymentMethod.Id);
                    }

                    // Small delay to avoid overwhelming the email service
                    await Task.Delay(TimeSpan.FromMilliseconds(100), linkedCts.Token);
                }

                pageNumber++;
            }

            _logger.LogInformation(
                "Completed expiring card notification processing. Pages processed: {Pages}, Success: {SuccessCount}, Failed: {FailureCount}",
                pageNumber - 1, successCount, failureCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ProcessExpiringCards operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("ProcessExpiringCards operation timed out after {Minutes} minutes", OperationTimeoutMinutes);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expiring payment cards");
            throw;
        }
        finally
        {
            // Clear tenant context
            tenantContext.ClearTenant();
        }
    }
}
