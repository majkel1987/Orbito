using MediatR;

namespace Orbito.Application.Clients.Commands.DeactivateClient
{
    public record DeactivateClientCommand(Guid Id) : IRequest<DeactivateClientResult>;
}
