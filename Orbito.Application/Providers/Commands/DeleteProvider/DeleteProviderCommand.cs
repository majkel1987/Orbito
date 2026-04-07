using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Commands.DeleteProvider;

/// <summary>
/// Command for deleting a provider.
/// Supports both soft delete (deactivation) and hard delete.
/// </summary>
public record DeleteProviderCommand(
    Guid Id,
    bool HardDelete = false
) : IRequest<Result<Unit>>;
