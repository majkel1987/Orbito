using System;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    public interface ISubscriptionRepository
    {
        // Read operations
        Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByIdForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByIdWithDetailsForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
        Task<Subscription?> GetByExternalIdForClientAsync(string externalId, Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetActiveSubscriptionsByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsByClientAsync(Guid clientId, DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsByClientAsync(Guid clientId, DateTime checkDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsByStatusForClientAsync(SubscriptionStatus status, Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdForClientAsync(Guid planId, Guid clientId, CancellationToken cancellationToken = default);

        // Search operations
        Task<IEnumerable<Subscription>> SearchSubscriptionsForClientAsync(string searchTerm, Guid clientId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        // CRUD operations
        Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default);

        // Business operations
        Task<bool> HasActiveSubscriptionAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default);
        Task<int> GetActiveSubscriptionsCountByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsForBillingByClientAsync(DateTime billingDate, Guid clientId, CancellationToken cancellationToken = default);

        // DEPRECATED: Admin-only operations (require special permissions and proper authorization)
        // WARNING: These methods expose data across all tenants - use with extreme caution
        [Obsolete("Use client-specific methods for better security. Only for admin operations with proper authorization.")]
        Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);

        [Obsolete("Use client-specific methods for better security. Only for admin operations with proper authorization.")]
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default);

        [Obsolete("Use client-specific methods for better security. Only for admin operations with proper authorization.")]
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime checkDate, CancellationToken cancellationToken = default);

        [Obsolete("Use client-specific methods for better security. Only for admin operations with proper authorization.")]
        Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default);

        Task<int> GetActiveSubscriptionsCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Subscription>> GetSubscriptionsForBillingAsync(DateTime billingDate, CancellationToken cancellationToken = default);

        // BACKGROUND JOB METHODS: Explicit TenantId for multi-tenant operations
        /// <summary>
        /// Gets active subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Subscription>> GetActiveSubscriptionsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets expiring subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsForTenantAsync(TenantId tenantId, DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets expired subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsForTenantAsync(TenantId tenantId, DateTime checkDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets subscriptions for billing for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Subscription>> GetSubscriptionsForBillingForTenantAsync(TenantId tenantId, DateTime billingDate, CancellationToken cancellationToken = default);
    }
}
