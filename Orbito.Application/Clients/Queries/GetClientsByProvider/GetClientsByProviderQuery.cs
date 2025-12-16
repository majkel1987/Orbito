using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using MediatR;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    /// <summary>
    /// Query to get clients by provider with filtering
    /// </summary>
    /// <param name="PageNumber">Page number (default: 1)</param>
    /// <param name="PageSize">Page size (default: 10)</param>
    /// <param name="Status">Status filter: 'active' for active only, 'inactive' for inactive only, null/empty for all</param>
    /// <param name="SearchTerm">Search term for filtering</param>
    public record GetClientsByProviderQuery(
        int PageNumber = 1,
        int PageSize = 10,
        string? Status = null,
        string? SearchTerm = null
    ) : IRequest<Orbito.Domain.Common.Result<PaginatedList<ClientDto>>>;
}
