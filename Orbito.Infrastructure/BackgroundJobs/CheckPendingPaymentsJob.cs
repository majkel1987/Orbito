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
    /// Background job for checking pending payments for all tenants
    /// </summary>
    public class CheckPendingPaymentsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CheckPendingPaymentsJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(15);

        public CheckPendingPaymentsJob(
            IServiceProvider serviceProvider,
            ILogger<CheckPendingPaymentsJob> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CheckPendingPaymentsJob started");

            // Initial delay to offset between jobs
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAllTenantsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking pending payments");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("CheckPendingPaymentsJob stopped");
        }

        private async Task CheckAllTenantsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentProcessingService>();
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

            // Get all active tenants (ignore query filters)
            var tenantIds = await context.Providers
                .IgnoreQueryFilters()
                .Where(p => p.IsActive)
                .Select(p => p.TenantId.Value)
                .Distinct()
                .ToListAsync(stoppingToken);

            _logger.LogDebug(
                "Checking pending payments for {TenantCount} tenants",
                tenantIds.Count);

            // Check pending payments for each tenant
            foreach (var tenantId in tenantIds)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromMinutes(10)); // 10 min timeout for 15 min job

                try
                {
                    // Set tenant context for this iteration
                    tenantProvider.SetTenantOverride(tenantId);

                    _logger.LogDebug("Checking pending payments for tenant {TenantId}", tenantId);

                    await paymentService.ValidatePaymentStatusAsync(cts.Token);

                    _logger.LogDebug(
                        "Successfully checked pending payments for tenant {TenantId}",
                        tenantId);
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Checking timed out for tenant {TenantId}",
                        tenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error checking pending payments for tenant {TenantId}",
                        tenantId);
                    // Continue with remaining tenants
                }
                finally
                {
                    // Clear tenant override
                    tenantProvider.ClearTenantOverride();
                }
            }

            _logger.LogDebug("Successfully checked pending payments for all tenants");
        }
    }
}
