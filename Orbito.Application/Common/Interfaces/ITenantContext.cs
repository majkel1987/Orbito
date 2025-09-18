using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    public interface ITenantContext
    {
        TenantId? CurrentTenantId { get; }
        void SetTenant(TenantId? tenantId);
        void ClearTenant();
        bool HasTenant { get; }
    }
}
