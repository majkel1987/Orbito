using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    /// <summary>
    /// Repository implementation for managing payment retry schedules
    /// </summary>
    public class PaymentRetryRepository : IPaymentRetryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PaymentRetryRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// Gets all retry schedules that are due for processing
        /// Includes:
        /// - Scheduled retries where NextAttemptAt <= now
        /// - InProgress retries that are stuck (LastAttemptAt older than inProgressTimeout)
        /// </summary>
        public async Task<List<PaymentRetrySchedule>> GetDueRetriesAsync(DateTime now, TimeSpan? inProgressTimeout = null, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return new List<PaymentRetrySchedule>();

            var tenantId = _tenantProvider.GetCurrentTenantId();
            var timeout = inProgressTimeout ?? TimeSpan.FromMinutes(30);
            var stuckThreshold = now.Subtract(timeout);

            return await _context.PaymentRetrySchedules
                .Where(r => r.TenantId == tenantId &&
                           (
                               // Normal scheduled retries
                               (r.Status == RetryStatus.Scheduled && r.NextAttemptAt <= now) ||
                               // Stuck InProgress retries (recovery mechanism)
                               (r.Status == RetryStatus.InProgress && r.UpdatedAt < stuckThreshold)
                           ))
                .OrderBy(r => r.NextAttemptAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets retry schedule by payment ID and client ID
        /// </summary>
        public async Task<PaymentRetrySchedule?> GetByPaymentIdAsync(Guid paymentId, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return null;

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Include(r => r.Payment)
                .Where(r => r.TenantId == tenantId &&
                           r.PaymentId == paymentId &&
                           r.ClientId == clientId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets retry schedule by ID for a specific client (SECURITY: prevents cross-client data leak)
        /// </summary>
        public async Task<PaymentRetrySchedule?> GetByIdForClientAsync(Guid scheduleId, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return null;

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Include(r => r.Payment)
                .Where(r => r.TenantId == tenantId &&
                           r.Id == scheduleId &&
                           r.ClientId == clientId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets active retry schedule for a payment
        /// </summary>
        public async Task<PaymentRetrySchedule?> GetActiveRetryByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return null;

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Where(r => r.TenantId == tenantId &&
                           r.PaymentId == paymentId &&
                           (r.Status == RetryStatus.Scheduled || r.Status == RetryStatus.InProgress))
                .OrderBy(r => r.NextAttemptAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all active retry schedules for a payment
        /// </summary>
        public async Task<List<PaymentRetrySchedule>> GetActiveRetriesByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return new List<PaymentRetrySchedule>();

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Where(r => r.TenantId == tenantId &&
                           r.PaymentId == paymentId &&
                           (r.Status == RetryStatus.Scheduled || r.Status == RetryStatus.InProgress))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets retry schedules for a payment
        /// </summary>
        public async Task<List<PaymentRetrySchedule>> GetRetrySchedulesByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return new List<PaymentRetrySchedule>();

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Where(r => r.TenantId == tenantId && r.PaymentId == paymentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the next retry schedule for a payment
        /// </summary>
        public async Task<PaymentRetrySchedule?> GetNextRetryByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return null;

            var tenantId = _tenantProvider.GetCurrentTenantId();

            return await _context.PaymentRetrySchedules
                .Where(r => r.TenantId == tenantId &&
                           r.PaymentId == paymentId &&
                           r.Status == RetryStatus.Scheduled)
                .OrderBy(r => r.NextAttemptAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Marks a retry schedule as processing (optimistic concurrency)
        /// </summary>
        public async Task<bool> MarkAsProcessingAsync(Guid scheduleId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return false;

            var tenantId = _tenantProvider.GetCurrentTenantId();

            var retry = await _context.PaymentRetrySchedules
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == scheduleId, cancellationToken);

            if (retry == null || retry.Status != RetryStatus.Scheduled)
            {
                return false;
            }

            retry.MarkAsInProgress();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Adds a new retry schedule
        /// </summary>
        public async Task AddAsync(PaymentRetrySchedule retrySchedule, CancellationToken cancellationToken = default)
        {
            await _context.PaymentRetrySchedules.AddAsync(retrySchedule, cancellationToken);
        }

        /// <summary>
        /// Updates an existing retry schedule
        /// </summary>
        public async Task UpdateAsync(PaymentRetrySchedule retrySchedule, CancellationToken cancellationToken = default)
        {
            _context.PaymentRetrySchedules.Update(retrySchedule);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets queryable retry schedules for filtering
        /// </summary>
        public async Task<IQueryable<PaymentRetrySchedule>> GetScheduledRetriesQueryAsync(Guid? clientId = null, string? status = null, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return Enumerable.Empty<PaymentRetrySchedule>().AsQueryable();

            var tenantId = _tenantProvider.GetCurrentTenantId();

            var query = _context.PaymentRetrySchedules
                .Include(rs => rs.Payment)
                .Where(rs => rs.TenantId == tenantId)
                .AsQueryable();

            if (clientId.HasValue)
            {
                query = query.Where(rs => rs.ClientId == clientId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<RetryStatus>(status, out var statusEnum))
            {
                query = query.Where(rs => rs.Status == statusEnum);
            }

            return await Task.FromResult(query);
        }

        /// <summary>
        /// Gets retry schedules by payment IDs for a specific client (SECURITY: prevents cross-client data leak)
        /// </summary>
        public async Task<List<PaymentRetrySchedule>> GetRetrySchedulesByPaymentIdsAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (!_tenantProvider.HasTenant())
                return new List<PaymentRetrySchedule>();

            var tenantId = _tenantProvider.GetCurrentTenantId();

            // SECURITY: Filter by BOTH TenantId AND ClientId to prevent cross-client data access
            return await _context.PaymentRetrySchedules
                .Where(rs => rs.TenantId == tenantId
                    && rs.ClientId == clientId
                    && paymentIds.Contains(rs.PaymentId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
