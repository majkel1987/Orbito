using MediatR;

namespace Orbito.Application.Clients.Commands.ActivateClient
{
    public record ActivateClientCommand(Guid Id) : IRequest<ActivateClientResult>;
}
