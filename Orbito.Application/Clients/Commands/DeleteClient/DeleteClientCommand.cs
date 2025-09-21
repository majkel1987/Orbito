using MediatR;

namespace Orbito.Application.Clients.Commands.DeleteClient
{
    public record DeleteClientCommand(
        Guid Id,
        bool HardDelete = false
    ) : IRequest<DeleteClientResult>;
}
