using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using PaginatedList = Orbito.Application.Common.Models.PaginatedList<Orbito.Application.DTOs.ClientDto>;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    /// <summary>
    /// Query to get clients by provider with filtering.
    /// </summary>
    /// <param name="PageNumber">Page number (default: 1).</param>
    /// <param name="PageSize">Page size (default: 10).</param>
    /// <param name="ActiveOnly">Status filter: null = all clients, true = active only, false = inactive only.</param>
    /// <param name="SearchTerm">Search term for filtering.</param>
    public record GetClientsByProviderQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool? ActiveOnly = null,
        string? SearchTerm = null
    ) : IRequest<Result<PaginatedList>>;
}
