using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Interfaces;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// All entities implementing this interface must have tenant isolation
/// enforced at the repository and query level.
/// </summary>
/// <remarks>
/// This interface is critical for multi-tenancy security. EF Core global
/// query filters should be configured to automatically filter by TenantId
/// for all entities implementing this interface.
/// </remarks>
public interface IMustHaveTenant
{
    /// <summary>
    /// Gets the tenant identifier this entity belongs to.
    /// </summary>
    TenantId TenantId { get; }
}
