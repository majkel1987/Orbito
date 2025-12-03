using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Queries.GetClientById
{
    public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;
}
