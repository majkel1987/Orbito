using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Queries.GetProviderByUserId
{
    public record GetProviderByUserIdQuery(Guid UserId) : IRequest<Result<ProviderDto>>;
}
