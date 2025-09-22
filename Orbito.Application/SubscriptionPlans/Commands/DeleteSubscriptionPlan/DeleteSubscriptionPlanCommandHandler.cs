using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan
{
    public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, DeleteSubscriptionPlanResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public DeleteSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<DeleteSubscriptionPlanResult> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to delete subscription plan");

            var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (subscriptionPlan == null)
                throw new InvalidOperationException($"Subscription plan with ID {request.Id} not found");

            // Check if plan can be deleted
            if (!subscriptionPlan.CanBeDeleted() && !request.HardDelete)
            {
                return new DeleteSubscriptionPlanResult
                {
                    Id = request.Id,
                    IsDeleted = false,
                    IsHardDelete = false,
                    Message = "Cannot delete subscription plan with active subscriptions. Use hard delete to force deletion."
                };
            }

            if (request.HardDelete)
            {
                await _unitOfWork.SubscriptionPlans.DeleteAsync(subscriptionPlan, cancellationToken);
            }
            else
            {
                subscriptionPlan.Deactivate();
                await _unitOfWork.SubscriptionPlans.UpdateAsync(subscriptionPlan, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteSubscriptionPlanResult
            {
                Id = request.Id,
                IsDeleted = true,
                IsHardDelete = request.HardDelete,
                Message = request.HardDelete ? "Subscription plan permanently deleted" : "Subscription plan deactivated"
            };
        }
    }
}
