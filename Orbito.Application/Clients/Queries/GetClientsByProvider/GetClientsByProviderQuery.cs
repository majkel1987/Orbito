using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using MediatR;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    public record GetClientsByProviderQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false,
        string? SearchTerm = null
    ) : IRequest<Orbito.Domain.Common.Result<PaginatedList<ClientDto>>>;
}
