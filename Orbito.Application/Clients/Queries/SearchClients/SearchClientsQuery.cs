using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    public record SearchClientsQuery(
        string SearchTerm,
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false
    ) : IRequest<Orbito.Domain.Common.Result<PaginatedList<ClientDto>>>;
}
