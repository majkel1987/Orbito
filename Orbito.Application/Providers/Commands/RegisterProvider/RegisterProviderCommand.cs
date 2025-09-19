using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.RegisterProvider
{
    public record RegisterProviderCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string BusinessName,
        string SubdomainSlug,
        string? Description = null,
        string? Avatar = null,
        string? CustomDomain = null
    ) : IRequest<RegisterProviderResult>;
}
