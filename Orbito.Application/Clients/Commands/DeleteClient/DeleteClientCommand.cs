using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.DeleteClient
{
    /// <summary>
    /// Command to delete a client.
    /// </summary>
    /// <param name="Id">Client ID to delete.</param>
    /// <param name="HardDelete">If true, permanently deletes from database. If false (default), performs soft delete (sets IsDeleted flag).</param>
    public record DeleteClientCommand(
        Guid Id,
        bool HardDelete = false
    ) : IRequest<Result<Unit>>;
}
