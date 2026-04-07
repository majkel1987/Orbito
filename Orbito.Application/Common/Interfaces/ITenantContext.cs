using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Context for managing tenant information during request processing
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant ID
    /// </summary>
    TenantId? CurrentTenantId { get; }

    /// <summary>
    /// Sets the current tenant context
    /// </summary>
    /// <param name="tenantId">Tenant ID to set</param>
    void SetTenant(TenantId? tenantId);

    /// <summary>
    /// Clears the current tenant context
    /// </summary>
    void ClearTenant();

    /// <summary>
    /// Indicates whether a tenant context is currently set
    /// </summary>
    bool HasTenant { get; }
}
