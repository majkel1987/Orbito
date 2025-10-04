using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Services
{
    /// <summary>
    /// Implementation of ITenantProvider that uses ITenantContext with thread safety
    /// </summary>
    public class TenantProvider : ITenantProvider
    {
        private readonly ITenantContext _tenantContext;
        private readonly AsyncLocal<Guid?> _overrideTenantId = new();

        public TenantProvider(ITenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public TenantId? GetCurrentTenantId()
        {
            // Priority: override > tenant context
            if (_overrideTenantId.Value.HasValue)
                return TenantId.Create(_overrideTenantId.Value.Value);
                
            return _tenantContext.CurrentTenantId;
        }

        public Guid GetCurrentTenantIdAsGuid()
        {
            // Priority: override > tenant context
            if (_overrideTenantId.Value.HasValue)
                return _overrideTenantId.Value.Value;
                
            var currentTenantId = _tenantContext.CurrentTenantId;
            
            if (currentTenantId == null)
                // Return Guid.Empty for admin context (no tenant filtering)
                return Guid.Empty;

            return currentTenantId.Value;
        }

        public bool HasTenant()
        {
            return _overrideTenantId.Value.HasValue || _tenantContext.HasTenant;
        }

        public void SetTenantOverride(Guid tenantId)
        {
            _overrideTenantId.Value = tenantId;
        }

        public void ClearTenantOverride()
        {
            _overrideTenantId.Value = null;
        }
    }
}
