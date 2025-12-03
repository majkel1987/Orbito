using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Services;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Background job for syncing payment statuses with Stripe
    /// </summary>
    public class PaymentStatusSyncJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentStatusSyncJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(30); // Run every 30 minutes
        private readonly TimeSpan _initialDelay;

        public PaymentStatusSyncJob(
            IServiceProvider serviceProvider,
            ILogger<PaymentStatusSyncJob> logger,
            TimeSpan? initialDelay = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _initialDelay = initialDelay ?? TimeSpan.FromMinutes(15);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentStatusSyncJob started");

            // Initial delay to offset between jobs
            await Task.Delay(_initialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncAllTenantsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while syncing payment statuses with Stripe");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("PaymentStatusSyncJob stopped");
        }

        /// <summary>
        /// Syncs payment statuses with Stripe for all tenants
        /// </summary>
        private async Task SyncAllTenantsAsync(CancellationToken stoppingToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentProcessingService>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
            var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();

            // Get all active tenants (ignore query filters)
            var tenantIds = await context.Providers
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Select(p => p.TenantId.Value)
                .Distinct()
                .ToListAsync(stoppingToken);

            var currentDate = dateTime.UtcNow;

            _logger.LogInformation(
                "Syncing payment statuses with Stripe for {TenantCount} tenants on date {Date}",
                tenantIds.Count,
                currentDate.Date);

            // Sync payment statuses for each tenant
            foreach (var tenantId in tenantIds)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromMinutes(25)); // 25 min timeout for 30 min job

                try
                {
                    // Set tenant context for this iteration
                    tenantProvider.SetTenantOverride(tenantId);

                    _logger.LogDebug("Syncing payment statuses for tenant {TenantId}", tenantId);

                    await paymentService.SyncPaymentStatusesWithStripeAsync(currentDate, cts.Token);

                    _logger.LogDebug(
                        "Successfully synced payment statuses for tenant {TenantId}",
                        tenantId);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Sync timed out for tenant {TenantId}",
                        tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error syncing payment statuses for tenant {TenantId}",
                        tenantId);
                    // Continue with remaining tenants
                }
                finally
                {
                    // Clear tenant override
                    tenantProvider.ClearTenantOverride();
                }
            }

            _logger.LogInformation(
                "Successfully synced payment statuses with Stripe for all tenants on date {Date}",
                currentDate.Date);
        }
    }
}
