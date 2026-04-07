using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;

public record GetSubscriptionPlanByIdQuery : IRequest<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; init; }
}
