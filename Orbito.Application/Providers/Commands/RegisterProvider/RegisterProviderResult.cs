namespace Orbito.Application.Providers.Commands.RegisterProvider;

/// <summary>
/// Result DTO for successful provider registration.
/// Error cases are handled by Result&lt;T&gt; pattern.
/// </summary>
public record RegisterProviderResult(
    Guid UserId,
    Guid ProviderId,
    Guid TenantId,
    string BusinessName,
    string SubdomainSlug
);
