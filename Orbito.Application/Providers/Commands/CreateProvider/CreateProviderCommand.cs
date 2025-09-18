using MediatR;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.CreateProvider
{
    public record CreateProviderCommand(
        Guid UserId,
        string BusinessName,
        string SubdomainSlug,
        string? Description = null,
        string? Avatar = null,
        string? CustomDomain = null
    ) : IRequest<CreateProviderResult>;
}
