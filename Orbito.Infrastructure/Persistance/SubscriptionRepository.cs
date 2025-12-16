using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public partial class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Original methods (admin access - require authorization)
        public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<Subscription?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<Subscription?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("External ID cannot be null or empty", nameof(externalId));

            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == externalId, cancellationToken);
        }

        // Security-enhanced methods with client verification
        public async Task<Subscription?> GetByIdForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id && s.ClientId == clientId, cancellationToken);
        }

        public async Task<Subscription?> GetByIdWithDetailsForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id && s.ClientId == clientId, cancellationToken);
        }

        public async Task<Subscription?> GetByExternalIdForClientAsync(string externalId, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                throw new ArgumentException("External ID cannot be null or empty", nameof(externalId));

            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == externalId && s.ClientId == clientId, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsByClientAsync(Guid clientId, DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default)
        {
            var expirationThreshold = checkDate.AddDays(daysBeforeExpiration);

            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate <= expirationThreshold)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsByClientAsync(Guid clientId, DateTime checkDate, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId &&
                           (s.Status == SubscriptionStatus.Expired ||
                            (s.Status == SubscriptionStatus.Active && s.NextBillingDate != default(DateTime) && s.NextBillingDate < checkDate)))
                .OrderByDescending(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusForClientAsync(SubscriptionStatus status, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId && s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdForClientAsync(Guid planId, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId && s.PlanId == planId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> SearchSubscriptionsForClientAsync(string searchTerm, Guid clientId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var (validPageNumber, validPageSize) = ValidatePagination(pageNumber, pageSize);

            var query = _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.Plan.Name.Contains(searchTerm) ||
                                        (s.ExternalSubscriptionId != null && s.ExternalSubscriptionId.Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((validPageNumber - 1) * validPageSize)
                .Take(validPageSize)
                .ToListAsync(cancellationToken);
        }

        // Business operations
        public async Task<bool> HasActiveSubscriptionAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.ClientId == clientId && s.Status == SubscriptionStatus.Active, cancellationToken);
        }

        public async Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default)
        {
            var existingSubscription = await _context.Subscriptions
                .AnyAsync(s => s.ClientId == clientId && s.PlanId == planId && s.Status == SubscriptionStatus.Active, cancellationToken);

            return !existingSubscription;
        }

        public async Task<int> GetActiveSubscriptionsCountByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .CountAsync(s => s.ClientId == clientId && s.Status == SubscriptionStatus.Active, cancellationToken);
        }

        public async Task<decimal> GetTotalRevenueByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Where(s => s.ClientId == clientId && s.CurrentPrice != null)
                .SumAsync(s => s.CurrentPrice!.Amount, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsForBillingByClientAsync(DateTime billingDate, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.ClientId == clientId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate <= billingDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        // DEPRECATED: Admin-only operations (require special permissions)
        public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default)
        {
            var expirationThreshold = checkDate.AddDays(daysBeforeExpiration);

            // Disable query filters for admin operations
            return await _context.Subscriptions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate <= expirationThreshold)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime checkDate, CancellationToken cancellationToken = default)
        {
            // Disable query filters for admin operations
            return await _context.Subscriptions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Expired ||
                           (s.Status == SubscriptionStatus.Active && s.NextBillingDate != default(DateTime) && s.NextBillingDate < checkDate))
                .OrderByDescending(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveSubscriptionsCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken);
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Where(s => s.CurrentPrice != null)
                .SumAsync(s => s.CurrentPrice!.Amount, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsForBillingAsync(DateTime billingDate, CancellationToken cancellationToken = default)
        {
            // Disable query filters for admin operations
            return await _context.Subscriptions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate <= billingDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        // CRUD operations
        public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Add(subscription);
            return subscription;
        }

        public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Update(subscription);
        }

        public async Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Remove(subscription);
        }

        /// <summary>
        /// Gets active subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .IgnoreQueryFilters() // Bypass query filters
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets expiring subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsForTenantAsync(TenantId tenantId, DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default)
        {
            var expirationDate = checkDate.AddDays(daysBeforeExpiration);

            return await _context.Subscriptions
                .IgnoreQueryFilters() // Bypass query filters
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate >= checkDate &&
                           s.NextBillingDate <= expirationDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets expired subscriptions for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsForTenantAsync(TenantId tenantId, DateTime checkDate, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .IgnoreQueryFilters() // Bypass query filters
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate < checkDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets subscriptions for billing for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Subscription>> GetSubscriptionsForBillingForTenantAsync(TenantId tenantId, DateTime billingDate, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .IgnoreQueryFilters() // Bypass query filters
                .AsNoTracking()
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.TenantId == tenantId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate != default(DateTime) &&
                           s.NextBillingDate <= billingDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        private static (int pageNumber, int pageSize) ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Security limit
            return (pageNumber, pageSize);
        }
    }
}