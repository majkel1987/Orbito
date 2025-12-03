using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.DeleteClient
{
    public record DeleteClientCommand(
        Guid Id,
        bool HardDelete = false
    ) : IRequest<Result<Unit>>;
}
