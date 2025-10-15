using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        using var scope = _serviceProvider.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetService<ISubscriptionService>();
        var dateTime = scope.ServiceProvider.GetService<IDateTime>();
        var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();

        if (subscriptionService == null || dateTime == null || tenantContext == null)
        {
            _logger.LogError("Required services not available");
            return;
        }

        var currentDate = dateTime.UtcNow;

        _logger.LogInformation("Processing recurring payments for date {Date}", currentDate.Date);

        var recurringPaymentsSuccess = false;
        var expiredSubscriptionsSuccess = false;

        try
        {
            // Set admin tenant context for background job
            // This allows access to all tenants' data for admin operations
            tenantContext.SetTenant(null); // Admin context - no tenant filtering

            // Create timeout for the operation
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(OperationTimeoutMinutes));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            // Process recurring payments for today
            try
            {
                await subscriptionService.ProcessRecurringPaymentsAsync(currentDate, linkedCts.Token);
                recurringPaymentsSuccess = true;
                _logger.LogInformation("Successfully processed recurring payments for date {Date}", currentDate.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process recurring payments for date {Date}", currentDate.Date);
            }

            // Process expired subscriptions
            try
            {
                await subscriptionService.ProcessExpiredSubscriptionsAsync(linkedCts.Token);
                expiredSubscriptionsSuccess = true;
                _logger.LogInformation("Successfully processed expired subscriptions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired subscriptions");
            }

            _logger.LogInformation(
                "Completed recurring payments job. Recurring payments: {RecurringStatus}, Expired subscriptions: {ExpiredStatus}",
                recurringPaymentsSuccess ? "Success" : "Failed",
                expiredSubscriptionsSuccess ? "Success" : "Failed");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ProcessRecurringPayments operation was cancelled");
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("ProcessRecurringPayments operation timed out after {Minutes} minutes", OperationTimeoutMinutes);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing recurring payments for date {Date}", currentDate.Date);
            throw;
        }
        finally
        {
            // Clear tenant context
            tenantContext.ClearTenant();
        }
    }
}
