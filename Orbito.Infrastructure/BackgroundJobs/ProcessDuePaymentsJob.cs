using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Services;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Background job for processing due payments for all tenants
    /// </summary>
    public class ProcessDuePaymentsJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessDuePaymentsJob> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);
        private readonly TimeSpan _initialDelay;
        
        // Health check properties
        private DateTime? _lastSuccessfulRun;
        private int _failedAttempts;

        public ProcessDuePaymentsJob(
            IServiceProvider serviceProvider,
            ILogger<ProcessDuePaymentsJob> logger,
            TimeSpan? initialDelay = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _initialDelay = initialDelay ?? TimeSpan.FromMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProcessDuePaymentsJob started");

            // Initial delay to offset between jobs
            await Task.Delay(_initialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAllTenantsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing due payments");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("ProcessDuePaymentsJob stopped");
        }

        /// <summary>
        /// Health check method for monitoring job status
        /// </summary>
        public bool IsHealthy()
        {
            if (_lastSuccessfulRun == null)
                return true; // New job, not checked yet

            var timeSinceLastRun = DateTime.UtcNow - _lastSuccessfulRun.Value;
            return timeSinceLastRun < TimeSpan.FromHours(2) && _failedAttempts < 3;
        }

        private async Task ProcessAllTenantsAsync(CancellationToken stoppingToken)
        {
            try
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

                _logger.LogInformation(
                    "Processing due payments for {TenantCount} tenants on date {Date}",
                    tenantIds.Count,
                    dateTime.UtcNow.Date);

                // Process payments in batches for better performance
                var tenantBatches = tenantIds
                    .Select((id, index) => new { id, index })
                    .GroupBy(x => x.index / 10) // 10 tenants per batch
                    .Select(g => g.Select(x => x.id).ToList())
                    .ToList();

                _logger.LogInformation(
                    "Processing {BatchCount} batches of tenants",
                    tenantBatches.Count);

                foreach (var batch in tenantBatches)
                {
                    var tasks = batch.Select(tenantId => ProcessTenantAsync(tenantId, tenantProvider, paymentService, dateTime, stoppingToken));
                    await Task.WhenAll(tasks);
                }

                _logger.LogInformation(
                    "Successfully processed due payments for all tenants on date {Date}",
                    dateTime.UtcNow.Date);

                // Update health check status
                _lastSuccessfulRun = DateTime.UtcNow;
                _failedAttempts = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessAllTenantsAsync");
                _failedAttempts++;
                throw;
            }
        }

        private async Task ProcessTenantAsync(
            Guid tenantId,
            ITenantProvider tenantProvider,
            IPaymentProcessingService paymentService,
            IDateTime dateTime,
            CancellationToken stoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            cts.CancelAfter(TimeSpan.FromMinutes(50)); // 50 min timeout for 1h job

            try
            {
                // Set tenant context for this iteration
                tenantProvider.SetTenantOverride(tenantId);

                _logger.LogDebug("Processing payments for tenant {TenantId}", tenantId);

                var tenantIdValueObject = TenantId.Create(tenantId);
                await paymentService.ProcessPendingPaymentsForTenantAsync(
                    tenantIdValueObject,
                    dateTime.UtcNow,
                    cts.Token);

                _logger.LogDebug(
                    "Successfully processed payments for tenant {TenantId}",
                    tenantId);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Processing timed out for tenant {TenantId}",
                    tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing payments for tenant {TenantId}",
                    tenantId);
                // Continue with remaining tenants
            }
            finally
            {
                // Clear tenant override
                tenantProvider.ClearTenantOverride();
            }
        }
    }
}
