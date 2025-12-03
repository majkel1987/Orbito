using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions
{
    public record GetExpiringSubscriptionsQuery : IRequest<Result<Common.Models.PaginatedList<SubscriptionDto>>>
    {
        public int DaysBeforeExpiration { get; init; } = 7;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
