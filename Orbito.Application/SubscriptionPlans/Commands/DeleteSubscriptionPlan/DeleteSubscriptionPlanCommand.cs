using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public record DeleteSubscriptionPlanCommand : IRequest<Result<DeleteSubscriptionPlanResult>>
{
    public Guid Id { get; init; }
    public bool HardDelete { get; init; } = false;
}
