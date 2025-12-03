using MediatR;
using Orbito.Application.DTOs;

namespace Orbito.Application.Providers.Queries.GetProviderById
{
    public record GetProviderByIdQuery(Guid Id) : IRequest<Orbito.Domain.Common.Result<ProviderDto>>;
}
