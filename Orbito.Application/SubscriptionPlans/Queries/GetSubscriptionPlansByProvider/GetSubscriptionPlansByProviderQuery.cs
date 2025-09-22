using MediatR;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public record GetSubscriptionPlansByProviderQuery : IRequest<SubscriptionPlansListDto>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public bool ActiveOnly { get; init; } = false;
        public bool PublicOnly { get; init; } = false;
        public string? SearchTerm { get; init; }
    }
}
