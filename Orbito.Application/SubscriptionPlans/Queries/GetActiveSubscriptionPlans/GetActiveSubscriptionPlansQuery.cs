using MediatR;

namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
    public record GetActiveSubscriptionPlansQuery : IRequest<ActiveSubscriptionPlansDto>
    {
        public bool PublicOnly { get; init; } = true;
        public int? Limit { get; init; }
    }
}
