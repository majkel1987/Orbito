using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ValidatePlanNameAsync(string name, Guid? excludePlanId = null, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SubscriptionPlans.IsPlanNameUniqueAsync(name, excludePlanId, cancellationToken);
        }

        public async Task<bool> CanPlanBeDeletedAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SubscriptionPlans.CanPlanBeDeletedAsync(planId, cancellationToken);
        }

        public async Task<SubscriptionPlan?> GetPlanWithMetricsAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.SubscriptionPlans.GetByIdWithSubscriptionsAsync(planId, cancellationToken);
        }

        public async Task<bool> IsPlanActiveAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(planId, cancellationToken);
            return plan?.IsActive ?? false;
        }

        public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
        {
            var plans = await _unitOfWork.SubscriptionPlans.GetPlansBySortOrderAsync(cancellationToken);
            return plans.Any() ? plans.Max(p => p.SortOrder) + 1 : 0;
        }

        public async Task ReorderPlansAsync(List<Guid> planIds, CancellationToken cancellationToken = default)
        {
            var plans = await _unitOfWork.SubscriptionPlans.GetPlansBySortOrderAsync(cancellationToken);
            var planDict = plans.ToDictionary(p => p.Id, p => p);

            for (int i = 0; i < planIds.Count; i++)
            {
                if (planDict.TryGetValue(planIds[i], out var plan))
                {
                    plan.UpdateSortOrder(i);
                    await _unitOfWork.SubscriptionPlans.UpdateAsync(plan, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
