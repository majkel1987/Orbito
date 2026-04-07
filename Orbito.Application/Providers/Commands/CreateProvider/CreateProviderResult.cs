namespace Orbito.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Result DTO for successful provider creation.
/// </summary>
public record CreateProviderResult(
    Guid ProviderId,
    Guid TenantId,
    string BusinessName,
    string SubdomainSlug,
    bool IsActive
);
