using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.ActivateClient
{
    public record ActivateClientCommand(Guid Id) : IRequest<Result<ClientDto>>;
}
