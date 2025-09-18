using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Services
{
    public class TenantContext : ITenantContext
    {
        private TenantId? _currentTenantId;

        public TenantId? CurrentTenantId => _currentTenantId;

        public bool HasTenant => _currentTenantId != null;

        public void SetTenant(TenantId? tenantId)
        {
            _currentTenantId = tenantId;
        }

        public void ClearTenant()
        {
            _currentTenantId = null;
        }
    }
}
