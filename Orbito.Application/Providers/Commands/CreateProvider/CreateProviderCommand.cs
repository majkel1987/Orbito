using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Command for creating a new provider for an existing user.
/// Used by admin/system flows to create providers for existing accounts.
/// </summary>
public record CreateProviderCommand(
    Guid UserId,
    string BusinessName,
    string SubdomainSlug,
    string Email,
    string FirstName,
    string LastName,
    Guid? SelectedPlatformPlanId = null,
    string? Description = null,
    string? Avatar = null,
    string? CustomDomain = null
) : IRequest<Result<CreateProviderResult>>;
