using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Interfaces
{
    public interface IMustHaveTenant
    {
        TenantId TenantId { get; }
    }
}
