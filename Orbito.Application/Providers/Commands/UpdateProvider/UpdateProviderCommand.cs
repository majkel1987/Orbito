using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.UpdateProvider;

/// <summary>
/// Command for updating an existing provider's information.
/// </summary>
public record UpdateProviderCommand(
    Guid Id,
    string BusinessName,
    string? Description = null,
    string? Avatar = null,
    string? SubdomainSlug = null,
    string? CustomDomain = null
) : IRequest<Result<ProviderDto>>;
