using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
    public record GetActiveSubscriptionPlansQuery : IRequest<Result<ActiveSubscriptionPlansDto>>
    {
        public bool PublicOnly { get; init; } = true;
        public int? Limit { get; init; }
    }
}
