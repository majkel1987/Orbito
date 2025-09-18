using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.CreateProvider
{
    public record CreateProviderResult(
        Guid ProviderId,
        TenantId TenantId,
        string BusinessName,
        string SubdomainSlug,
        bool IsActive
    );
}
