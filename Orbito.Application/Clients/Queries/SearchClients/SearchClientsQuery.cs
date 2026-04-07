using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using PaginatedList = Orbito.Application.Common.Models.PaginatedList<Orbito.Application.DTOs.ClientDto>;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    /// <summary>
    /// Query to search clients by term with pagination.
    /// </summary>
    /// <param name="SearchTerm">The search term to filter clients.</param>
    /// <param name="PageNumber">Page number (default: 1).</param>
    /// <param name="PageSize">Page size (default: 10).</param>
    /// <param name="ActiveOnly">If true, returns only active clients.</param>
    public record SearchClientsQuery(
        string SearchTerm,
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false
    ) : IRequest<Result<PaginatedList>>;
}
