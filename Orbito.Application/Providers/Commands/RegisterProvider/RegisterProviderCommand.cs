using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.RegisterProvider;

/// <summary>
/// Command for registering a new provider with a new user account.
/// This is the self-registration flow for new providers.
/// </summary>
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
) : IRequest<Result<RegisterProviderResult>>;
