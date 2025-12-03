using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Queries.GetAllProviders
{
    public record GetAllProvidersQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false
    ) : IRequest<Domain.Common.Result<PaginatedList<ProviderDto>>>;
}
