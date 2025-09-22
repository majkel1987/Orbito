using MediatR;

namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan
{
    public record DeleteSubscriptionPlanCommand : IRequest<DeleteSubscriptionPlanResult>
    {
        public Guid Id { get; init; }
        public bool HardDelete { get; init; } = false;
    }
}
