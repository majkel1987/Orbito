using MediatR;

namespace Orbito.Application.Providers.Commands.RegisterProvider
{
    public record RegisterProviderCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string BusinessName,
        string SubdomainSlug,
        Guid? SelectedPlatformPlanId = null,
        string? Description = null,
        string? Avatar = null,
        string? CustomDomain = null
    ) : IRequest<RegisterProviderResult>;
}
