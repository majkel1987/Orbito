using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    public interface ISubscriptionPlanRepository
    {
        // Read operations
        Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<SubscriptionPlan?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SubscriptionPlan>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool activeOnly = false, bool publicOnly = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync(bool publicOnly = false, int? limit = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<SubscriptionPlan>> GetPlansBySortOrderAsync(CancellationToken cancellationToken = default);

        // Count operations
        Task<int> GetCountAsync(string? searchTerm = null, bool activeOnly = false, bool publicOnly = false, CancellationToken cancellationToken = default);

        // CRUD operations
        Task<SubscriptionPlan> AddAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default);
        Task UpdateAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default);
        Task DeleteAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default);

        // Business operations
        Task<bool> CanPlanBeDeletedAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<bool> IsPlanNameUniqueAsync(string name, Guid? excludePlanId = null, CancellationToken cancellationToken = default);
    }
}
