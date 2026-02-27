using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Portal.Queries.GetMySubscriptions
{
    public record GetMySubscriptionsQuery : IRequest<Result<List<SubscriptionDto>>>
    {
    }
}
