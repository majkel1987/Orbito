using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<bool> ValidatePlanNameAsync(string name, Guid? excludePlanId = null, CancellationToken cancellationToken = default);
        Task<bool> CanPlanBeDeletedAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<SubscriptionPlan?> GetPlanWithMetricsAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<bool> IsPlanActiveAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default);
        Task ReorderPlansAsync(List<Guid> planIds, CancellationToken cancellationToken = default);
    }
}
