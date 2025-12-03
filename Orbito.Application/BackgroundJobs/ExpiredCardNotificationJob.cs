using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Helpers;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

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
        _logger.LogInformation("Processing expired payment cards");

        // Create timeout for the operation
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Execute for all tenants
        var results = await TenantJobHelper.ExecuteForAllTenantsAsync(
            _serviceProvider,
            _logger,
            async (tenantId, serviceProvider, ct) =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var notificationService = serviceProvider.GetRequiredService<IPaymentNotificationService>();
                var tenantIdValueObject = TenantId.Create(tenantId);

                // Get expired payment methods for this tenant
                var expiredPaymentMethods = await unitOfWork.PaymentMethods
                    .GetExpiredPaymentMethodsForTenantAsync(tenantIdValueObject, ct);

                var expiredMethodsList = expiredPaymentMethods.ToList();
                _logger.LogDebug("Found {Count} expired payment methods for tenant {TenantId}",
                    expiredMethodsList.Count, tenantId);

                var successCount = 0;
                var failureCount = 0;

                foreach (var paymentMethod in expiredMethodsList)
                {
                    try
                    {
                        // Send expired card notification
                        await notificationService.SendExpiredCardNotificationAsync(
                            paymentMethod.Id,
                            ct);

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
                    await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                }

                _logger.LogDebug(
                    "Completed expired card notifications for tenant {TenantId}. Success: {SuccessCount}, Failed: {FailureCount}",
                    tenantId, successCount, failureCount);
            },
            linkedCts.Token);

        var successCount = results.Values.Count(r => r);
        _logger.LogInformation(
            "Completed expired card notification processing. Success: {SuccessCount}/{TotalCount}",
            successCount,
            results.Count);
    }

    /// <summary>
    /// Processes and sends notifications for cards expiring soon (within 30 days)
    /// </summary>
    private async Task ProcessExpiringCardsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

        var currentDate = dateTime.UtcNow;
        var expiryThresholdDate = currentDate.AddDays(DaysBeforeExpiryToNotify);

        _logger.LogInformation("Processing payment cards expiring before {ThresholdDate}", expiryThresholdDate.Date);

        // Create timeout for the operation
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        // Execute for all tenants
        var results = await TenantJobHelper.ExecuteForAllTenantsAsync(
            _serviceProvider,
            _logger,
            async (tenantId, serviceProvider, ct) =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var notificationService = serviceProvider.GetRequiredService<IPaymentNotificationService>();
                var dateTimeService = serviceProvider.GetRequiredService<IDateTime>();
                var currentDateLocal = dateTimeService.UtcNow;
                var expiryThresholdDateLocal = currentDateLocal.AddDays(DaysBeforeExpiryToNotify);

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
                        cancellationToken: ct);

                    var paymentMethodsList = paymentMethods.ToList();
                    hasMore = paymentMethodsList.Count == pageSize;

                    // Filter for cards expiring within the threshold AND belonging to this tenant
                    var expiringPaymentMethods = paymentMethodsList
                        .Where(pm => pm.ExpiryDate.HasValue &&
                                     !pm.IsExpired() &&
                                     pm.ExpiryDate.Value.Date <= expiryThresholdDateLocal.Date &&
                                     pm.Client.TenantId.Value == tenantId) // SECURITY: Filter by tenant
                        .ToList();

                    _logger.LogDebug("Processing page {Page} for tenant {TenantId}: found {Count} expiring cards out of {Total} cards in batch",
                        pageNumber, tenantId, expiringPaymentMethods.Count, paymentMethodsList.Count);

                    foreach (var paymentMethod in expiringPaymentMethods)
                    {
                        try
                        {
                            // Calculate days until expiry
                            var daysUntilExpiry = (int)(paymentMethod.ExpiryDate!.Value.Date - currentDateLocal.Date).TotalDays;

                            // Send card expiring soon notification
                            await notificationService.SendCardExpiringSoonNotificationAsync(
                                paymentMethod.Id,
                                daysUntilExpiry,
                                ct);

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
                        await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                    }

                    pageNumber++;
                }

                _logger.LogDebug(
                    "Completed expiring card notifications for tenant {TenantId}. Pages processed: {Pages}, Success: {SuccessCount}, Failed: {FailureCount}",
                    tenantId, pageNumber - 1, successCount, failureCount);
            },
            linkedCts.Token);

        var successCount = results.Values.Count(r => r);
        _logger.LogInformation(
            "Completed expiring card notification processing. Success: {SuccessCount}/{TotalCount}",
            successCount,
            results.Count);
    }
}
