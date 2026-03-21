using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.CreateProvider;

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
