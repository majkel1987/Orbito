using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Common.Helpers;

/// <summary>
/// Helper class for executing operations in tenant context for background jobs
/// Provides safe tenant isolation and error handling
/// </summary>
public static class TenantJobHelper
{
    /// <summary>
    /// Executes an operation for each active tenant
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Operation to execute per tenant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with tenant ID as key and success status as value</returns>
    public static async Task<Dictionary<Guid, bool>> ExecuteForAllTenantsAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        Func<Guid, IServiceProvider, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Guid, bool>();

        await using var scope = serviceProvider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Get all active tenant IDs without loading full Provider entities
        // Memory-optimized: extract only TenantId values
        var tenantIds = new HashSet<Guid>();
        var pageNumber = 1;
        const int pageSize = 100;
        var hasMore = true;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            var providers = await unitOfWork.Providers.GetActiveProvidersAsync(pageNumber, pageSize, cancellationToken);
            var providersList = providers.ToList();

            // Extract tenant IDs immediately, don't keep full entities
            foreach (var provider in providersList)
            {
                tenantIds.Add(provider.TenantId.Value);
            }

            hasMore = providersList.Count == pageSize;
            pageNumber++;
        }

        var tenantIdsList = tenantIds.ToList();

        logger.LogInformation(
            "Executing operation for {TenantCount} tenants",
            tenantIdsList.Count);

        // Process tenants in batches for better performance
        const int batchSize = 10;
        var tenantBatches = tenantIdsList
            .Select((id, index) => new { id, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.id).ToList())
            .ToList();

        foreach (var batch in tenantBatches)
        {
            var tasks = batch.Select(async tenantId =>
            {
                try
                {
                    await ExecuteForTenantAsync(
                        tenantId,
                        serviceProvider,
                        logger,
                        operation,
                        cancellationToken);
                    results[tenantId] = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Error executing operation for tenant {TenantId}",
                        tenantId);
                    results[tenantId] = false;
                }
            });

            await Task.WhenAll(tasks);
        }

        var successCount = results.Values.Count(r => r);
        logger.LogInformation(
            "Completed operation for all tenants. Success: {SuccessCount}/{TotalCount}",
            successCount,
            tenantIdsList.Count);

        return results;
    }

    /// <summary>
    /// Executes an operation for a specific tenant with proper context setup
    /// </summary>
    /// <param name="tenantId">Tenant ID to execute operation for</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ExecuteForTenantAsync(
        Guid tenantId,
        IServiceProvider serviceProvider,
        ILogger logger,
        Func<Guid, IServiceProvider, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

        try
        {
            // Set tenant context for this operation
            tenantProvider.SetTenantOverride(tenantId);

            logger.LogDebug("Executing operation for tenant {TenantId}", tenantId);

            await operation(tenantId, scope.ServiceProvider, cancellationToken);

            logger.LogDebug("Successfully executed operation for tenant {TenantId}", tenantId);
        }
        finally
        {
            // Always clear tenant override
            tenantProvider.ClearTenantOverride();
        }
    }

    /// <summary>
    /// Executes an operation that requires admin context (no tenant filtering)
    /// Use with caution - only for operations that truly need cross-tenant access
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ExecuteInAdminContextAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        Func<IServiceProvider, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        try
        {
            // Set admin context (no tenant)
            tenantContext.SetTenant(null);

            logger.LogDebug("Executing operation in admin context");

            await operation(scope.ServiceProvider, cancellationToken);

            logger.LogDebug("Successfully executed operation in admin context");
        }
        finally
        {
            // Always clear tenant context
            tenantContext.ClearTenant();
        }
    }
}

