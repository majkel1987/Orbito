using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.DeactivateClient
{
    public record DeactivateClientCommand(Guid Id) : IRequest<Result<ClientDto>>;
}
