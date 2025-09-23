using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        // Read operations
        Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime checkDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        // Search operations
        Task<IEnumerable<Subscription>> SearchSubscriptionsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        // CRUD operations
        Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default);

        // Business operations
        Task<bool> HasActiveSubscriptionAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default);
        Task<int> GetActiveSubscriptionsCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsForBillingAsync(DateTime billingDate, CancellationToken cancellationToken = default);
    }
}
