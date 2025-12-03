using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistence
{
    /// <summary>
    /// Repository implementation for payment method operations
    /// </summary>
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;

        public PaymentMethodRepository(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        /// <summary>
        /// Gets payment method by ID with client verification
        /// </summary>
        public async Task<PaymentMethod?> GetByIdAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == id && pm.ClientId == clientId, cancellationToken);
        }

        /// <summary>
        /// Gets payment methods by client ID
        /// </summary>
        public async Task<IEnumerable<PaymentMethod>> GetByClientIdAsync(
            Guid clientId,
            int pageNumber = 1,
            int pageSize = 10,
            PaymentMethodType? type = null,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentMethods
                .Where(pm => pm.ClientId == clientId);

            if (type.HasValue)
            {
                query = query.Where(pm => pm.Type == type.Value);
            }

            if (activeOnly)
            {
                query = query.Where(pm => !pm.IsExpired() && pm.CanBeUsed());
            }

            return await query
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets payment methods with count by client ID (optimized single query)
        /// </summary>
        public async Task<(IEnumerable<PaymentMethod> PaymentMethods, int TotalCount)> GetByClientIdWithCountAsync(
            Guid clientId,
            int pageNumber = 1,
            int pageSize = 10,
            PaymentMethodType? type = null,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentMethods
                .Where(pm => pm.ClientId == clientId);

            if (type.HasValue)
            {
                query = query.Where(pm => pm.Type == type.Value);
            }

            if (activeOnly)
            {
                query = query.Where(pm => !pm.IsExpired() && pm.CanBeUsed());
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Get paginated data
            var paymentMethods = await query
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (paymentMethods, totalCount);
        }

        /// <summary>
        /// Gets default payment methods by client ID
        /// </summary>
        public async Task<IEnumerable<PaymentMethod>> GetDefaultPaymentMethodsByClientAsync(
            Guid clientId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.ClientId == clientId && pm.IsDefault)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets payment methods by type
        /// </summary>
        public async Task<IEnumerable<PaymentMethod>> GetByTypeAsync(
            PaymentMethodType type, 
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentMethods
                .Where(pm => pm.Type == type)
                .OrderByDescending(pm => pm.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets expired payment methods
        /// SECURITY: Returns ALL expired payment methods across ALL tenants (admin-only operation)
        /// This method is used by ExpiredCardNotificationJob which processes all tenants
        /// </summary>
        [Obsolete("ADMIN-ONLY: Returns expired payment methods from ALL tenants. Use GetExpiredPaymentMethodsForTenantAsync for tenant-specific operations.")]
        public async Task<IEnumerable<PaymentMethod>> GetExpiredPaymentMethodsAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY NOTICE: This method does NOT filter by tenant
            // It's used by background jobs that iterate through ALL tenants
            // Each tenant's payment methods are processed separately in the job

            var currentDate = DateTime.UtcNow;

            return await _context.PaymentMethods
                .Include(pm => pm.Client) // Need tenant info for processing
                .Where(pm => pm.ExpiryDate.HasValue && pm.ExpiryDate.Value < currentDate)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets expired payment methods for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<PaymentMethod>> GetExpiredPaymentMethodsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow;

            return await _context.PaymentMethods
                .Include(pm => pm.Client) // Need client info for notifications
                .Where(pm => pm.ExpiryDate.HasValue && 
                            pm.ExpiryDate.Value < currentDate &&
                            pm.Client.TenantId == tenantId) // SECURITY: Filter by tenant
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets count of payment methods by client ID
        /// </summary>
        public async Task<int> GetCountByClientIdAsync(
            Guid clientId, 
            PaymentMethodType? type = null, 
            bool activeOnly = true, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.PaymentMethods
                .Where(pm => pm.ClientId == clientId);

            if (type.HasValue)
            {
                query = query.Where(pm => pm.Type == type.Value);
            }

            if (activeOnly)
            {
                query = query.Where(pm => !pm.IsExpired() && pm.CanBeUsed());
            }

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new payment method
        /// </summary>
        public async Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
            return paymentMethod;
        }

        /// <summary>
        /// Updates an existing payment method
        /// </summary>
        public async Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
        {
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes a payment method
        /// </summary>
        public async Task DeleteAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
        {
            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Checks if client can add more payment methods
        /// </summary>
        public async Task<bool> CanAddPaymentMethodAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            const int maxPaymentMethodsPerClient = 10; // Configurable limit
            var currentCount = await GetActiveCountByClientIdAsync(clientId, cancellationToken);
            return currentCount < maxPaymentMethodsPerClient;
        }

        /// <summary>
        /// Gets count of active payment methods by client ID
        /// </summary>
        public async Task<int> GetActiveCountByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentMethods
                .CountAsync(pm => pm.ClientId == clientId && pm.IsDefault, cancellationToken);
        }
    }
}
