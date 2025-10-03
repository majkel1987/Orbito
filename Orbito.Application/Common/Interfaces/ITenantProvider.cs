using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Provides current tenant context for Entity Framework query filters
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// Gets the current tenant ID for query filtering
        /// </summary>
        /// <returns>Current tenant ID or null if no tenant context</returns>
        TenantId? GetCurrentTenantId();
        
        /// <summary>
        /// Gets the current tenant ID as Guid for EF Core query filters
        /// </summary>
        /// <returns>Current tenant ID as Guid</returns>
        /// <exception cref="InvalidOperationException">Thrown when tenant context is not available</exception>
        Guid GetCurrentTenantIdAsGuid();
        
        /// <summary>
        /// Checks if tenant context is available
        /// </summary>
        /// <returns>True if tenant is set, false otherwise</returns>
        bool HasTenant();
        
        /// <summary>
        /// Sets tenant override for background jobs (bypasses normal tenant resolution)
        /// </summary>
        /// <param name="tenantId">Tenant ID to override with</param>
        void SetTenantOverride(Guid tenantId);
        
        /// <summary>
        /// Clears tenant override
        /// </summary>
        void ClearTenantOverride();
    }
}
