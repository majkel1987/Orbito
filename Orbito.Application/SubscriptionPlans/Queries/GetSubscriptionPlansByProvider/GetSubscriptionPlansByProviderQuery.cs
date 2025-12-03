using MediatR;
using Orbito.Application.Common.Models;
using Orbito.Domain.Common;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public record GetSubscriptionPlansByProviderQuery : IRequest<Result<PaginatedList<SubscriptionPlanListItemDto>>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public bool ActiveOnly { get; init; } = false;
        public bool PublicOnly { get; init; } = false;
        public string? SearchTerm { get; init; }
    }
}
