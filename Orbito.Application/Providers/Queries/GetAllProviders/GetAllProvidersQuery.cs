using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;

namespace Orbito.Application.Providers.Queries.GetAllProviders;

/// <summary>
/// Query for retrieving all providers with pagination.
/// </summary>
public record GetAllProvidersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool ActiveOnly = false
) : IRequest<Orbito.Domain.Common.Result<PaginatedList<ProviderDto>>>;
