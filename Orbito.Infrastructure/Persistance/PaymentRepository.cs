using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<PaymentRepository> _logger;
        private readonly ISecurityLimitService _securityLimitService;

        public PaymentRepository(
            ApplicationDbContext context,
            ITenantContext tenantContext,
            ILogger<PaymentRepository> logger,
            ISecurityLimitService securityLimitService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityLimitService = securityLimitService ?? throw new ArgumentNullException(nameof(securityLimitService));
        }

        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use GetByIdForClientAsync instead.")]
        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called for payment {PaymentId}. This method should not be used!",
                nameof(GetByIdAsync), id);

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetByIdAsync));
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use GetByExternalTransactionIdForClientAsync instead.")]
        public async Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalTransactionId))
                throw new ArgumentException("External transaction ID cannot be null or empty", nameof(externalTransactionId));

            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called for transaction {TransactionId}. This method should not be used!",
                nameof(GetByExternalTransactionIdAsync), externalTransactionId);

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetByExternalTransactionIdAsync));
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId, cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method allows access to payments from ALL tenants without verification. Use tenant-specific methods instead.")]
        public async Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalPaymentId))
                throw new ArgumentException("External payment ID cannot be null or empty", nameof(externalPaymentId));

            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called for payment {PaymentId}. This method should not be used!",
                nameof(GetByExternalPaymentIdAsync), externalPaymentId);

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetByExternalPaymentIdAsync));
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("SECURITY: {MethodName} called without tenant context for subscription {SubscriptionId}",
                    nameof(GetBySubscriptionIdAsync), subscriptionId);
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId && // SECURITY: Explicit tenant filter
                            p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetByClientIdAsync(Guid clientId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.ClientId == clientId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payments from ALL clients without verification. Use client-specific methods instead.")]
        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called for status {Status}. This method should not be used!",
                nameof(GetByStatusAsync), status);

            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetByStatusAsync));
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;
            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == status && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("ADMIN-ONLY: Returns pending payments from ALL clients. Use client-specific methods for regular operations.")]
        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SECURITY: ADMIN-ONLY {MethodName} called. This returns data from ALL tenants!",
                nameof(GetPendingPaymentsAsync));

            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetPendingPaymentsAsync));
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Pending" && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("ADMIN-ONLY: Returns failed payments from ALL clients. Use client-specific methods for regular operations.")]
        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SECURITY: ADMIN-ONLY {MethodName} called. This returns data from ALL tenants!",
                nameof(GetFailedPaymentsAsync));

            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetFailedPaymentsAsync));
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Failed" && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderByDescending(p => p.FailedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns processing payments from ALL tenants without verification. Use tenant-specific methods instead.")]
        public async Task<IEnumerable<Payment>> GetProcessingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called. This method should not be used!",
                nameof(GetProcessingPaymentsAsync));

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetProcessingPaymentsAsync));
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Processing" && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payments with external IDs from ALL tenants without verification. Use tenant-specific methods instead.")]
        public async Task<IEnumerable<Payment>> GetPaymentsWithExternalIdAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called. This method should not be used!",
                nameof(GetPaymentsWithExternalIdAsync));

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetPaymentsWithExternalIdAsync));
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => !string.IsNullOrEmpty(p.ExternalPaymentId) && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var entry = await _context.Payments.AddAsync(payment, cancellationToken);
            return entry.Entity;
        }

        public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _context.Payments.Remove(payment);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.AnyAsync(p => p.ExternalTransactionId == externalTransactionId, cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payment stats from ALL tenants without verification. Use GetPaymentStatsByClientAsync instead.")]
        public async Task<PaymentStats> GetPaymentStatsAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called. This method should not be used!",
                nameof(GetPaymentStatsAsync));

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetPaymentStatsAsync));
                return new PaymentStats
                {
                    TotalPayments = 0,
                    CompletedPayments = 0,
                    FailedPayments = 0,
                    PendingPayments = 0,
                    ProcessingPayments = 0,
                    RefundedPayments = 0,
                    TotalRevenue = 0,
                    TotalRefunded = 0,
                    Currency = "USD",
                    LastUpdated = DateTime.UtcNow
                };
            }

            var tenantId = _tenantContext.CurrentTenantId;

            // Single query to get all stats at once
            // IMPORTANT: Use EF.Property<string> for enum comparisons to avoid InvalidCastException
            var stats = await _context.Payments
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPayments = g.Count(),
                    CompletedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Completed"),
                    FailedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Failed"),
                    PendingPayments = g.Count(p => EF.Property<string>(p, "Status") == "Pending"),
                    ProcessingPayments = g.Count(p => EF.Property<string>(p, "Status") == "Processing"),
                    RefundedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Refunded"),
                    TotalRevenue = g.Where(p => EF.Property<string>(p, "Status") == "Completed" && p.Amount != null)
                                   .Sum(p => (decimal?)p.Amount!.Amount) ?? 0,
                    TotalRefunded = g.Where(p => (EF.Property<string>(p, "Status") == "Refunded" ||
                                                 EF.Property<string>(p, "Status") == "PartiallyRefunded") &&
                                                 p.Amount != null)
                                    .Sum(p => (decimal?)p.Amount!.Amount) ?? 0,
                    MostCommonCurrency = g.Where(p => EF.Property<string>(p, "Status") == "Completed" && p.Amount != null)
                                          .GroupBy(p => p.Amount!.Currency)
                                          .OrderByDescending(c => c.Count())
                                          .Select(c => c.Key)
                                          .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stats == null)
            {
                return new PaymentStats
                {
                    TotalPayments = 0,
                    CompletedPayments = 0,
                    FailedPayments = 0,
                    PendingPayments = 0,
                    ProcessingPayments = 0,
                    RefundedPayments = 0,
                    TotalRevenue = 0,
                    TotalRefunded = 0,
                    Currency = "USD",
                    LastUpdated = DateTime.UtcNow
                };
            }

            return new PaymentStats
            {
                TotalPayments = stats.TotalPayments,
                CompletedPayments = stats.CompletedPayments,
                FailedPayments = stats.FailedPayments,
                PendingPayments = stats.PendingPayments,
                ProcessingPayments = stats.ProcessingPayments,
                RefundedPayments = stats.RefundedPayments,
                TotalRevenue = stats.TotalRevenue,
                TotalRefunded = stats.TotalRefunded,
                Currency = stats.MostCommonCurrency ?? "USD",
                LastUpdated = DateTime.UtcNow
            };
        }

        [Obsolete("SECURITY RISK: This method returns total revenue from ALL tenants without verification. Use GetTotalRevenueByClientAsync instead.")]
        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called. This method should not be used!",
                nameof(GetTotalRevenueAsync));

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetTotalRevenueAsync));
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .Where(p => EF.Property<string>(p, "Status") == "Completed" && p.Amount != null && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .SumAsync(p => p.Amount!.Amount, cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payment counts from ALL tenants without verification. Use GetPaymentsCountByStatusForClientAsync instead.")]
        public async Task<int> GetPaymentsCountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            _logger.LogWarning("SECURITY: Deprecated {MethodName} called for status {Status}. This method should not be used!",
                nameof(GetPaymentsCountByStatusAsync), status);

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                _logger.LogError("SECURITY: No tenant context available - access denied in {MethodName}",
                    nameof(GetPaymentsCountByStatusAsync));
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            // IMPORTANT: Convert enum to string for EF Core ValueConverter compatibility
            var statusString = status.ToString();
            return await _context.Payments.CountAsync(p => EF.Property<string>(p, "Status") == statusString && p.TenantId == tenantId, cancellationToken); // SECURITY: Filter by tenant
        }

        public async Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.CountAsync(p => p.SubscriptionId == subscriptionId, cancellationToken);
        }

        public async Task<Payment?> GetRecentBySubscriptionIdAsync(Guid subscriptionId, TimeSpan timeWindow, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);

            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.SubscriptionId == subscriptionId &&
                           p.CreatedAt >= cutoffTime)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Gets total refunded amount for a payment
        /// WARNING: This is a simplified implementation. For production, implement proper Refunds table.
        /// </summary>
        public async Task<decimal> GetTotalRefundedAmountAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            // IMPLEMENTATION NOTE: This method provides estimated refund amounts based on Payment.Status
            // Limitations:
            // 1. PartiallyRefunded status returns 50% estimate (not accurate)
            // 2. Cannot track multiple partial refunds
            // 3. No audit trail of refund transactions
            //
            // RECOMMENDED IMPLEMENTATION:
            // Create a Refunds table:
            // - CREATE TABLE Refunds (
            //     Id GUID PRIMARY KEY,
            //     PaymentId GUID NOT NULL,
            //     Amount DECIMAL NOT NULL,
            //     Status ENUM (Pending, Completed, Failed),
            //     RefundedAt DATETIME,
            //     Reason NVARCHAR(500),
            //     FOREIGN KEY (PaymentId) REFERENCES Payments(Id)
            //   )
            // - Track each refund transaction separately
            // - Sum up completed refunds for accurate totals

            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("GetTotalRefundedAmountAsync: Payment {PaymentId} not found", paymentId);
                return 0;
            }

            // Simplified logic based on status
            return payment.Status switch
            {
                PaymentStatus.Refunded => payment.Amount?.Amount ?? 0,
                PaymentStatus.PartiallyRefunded => (payment.Amount?.Amount ?? 0) * 0.5m, // ESTIMATE ONLY!
                _ => 0
            };
        }

        // Security-enhanced methods with client verification
        public async Task<Payment?> GetByIdForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
        {
            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("SECURITY: {MethodName} called without tenant context for payment {PaymentId}",
                    nameof(GetByIdForClientAsync), id);
                return null;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant first
                .FirstOrDefaultAsync(p => p.Id == id && p.ClientId == clientId, cancellationToken);
        }

        public async Task<Payment?> GetByExternalTransactionIdForClientAsync(string externalTransactionId, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalTransactionId))
                throw new ArgumentException("External transaction ID cannot be null or empty", nameof(externalTransactionId));

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId && p.ClientId == clientId, cancellationToken);
        }

        public async Task<PaymentStats> GetPaymentStatsByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            // IMPORTANT: Use EF.Property<string> for enum comparisons to avoid InvalidCastException
            var stats = await _context.Payments
                .Where(p => p.ClientId == clientId)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPayments = g.Count(),
                    CompletedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Completed"),
                    FailedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Failed"),
                    PendingPayments = g.Count(p => EF.Property<string>(p, "Status") == "Pending"),
                    ProcessingPayments = g.Count(p => EF.Property<string>(p, "Status") == "Processing"),
                    RefundedPayments = g.Count(p => EF.Property<string>(p, "Status") == "Refunded" || EF.Property<string>(p, "Status") == "PartiallyRefunded"),
                    TotalRevenue = g.Where(p => EF.Property<string>(p, "Status") == "Completed" && p.Amount != null).Sum(p => p.Amount!.Amount),
                    TotalRefunded = g.Where(p => (EF.Property<string>(p, "Status") == "Refunded" || EF.Property<string>(p, "Status") == "PartiallyRefunded") && p.Amount != null).Sum(p => p.Amount!.Amount),
                    MostCommonCurrency = g.Where(p => p.Amount != null).GroupBy(p => p.Amount!.Currency).OrderByDescending(gc => gc.Count()).Select(gc => gc.Key).FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stats == null)
            {
                return new PaymentStats
                {
                    TotalPayments = 0,
                    CompletedPayments = 0,
                    FailedPayments = 0,
                    PendingPayments = 0,
                    ProcessingPayments = 0,
                    RefundedPayments = 0,
                    TotalRevenue = 0,
                    TotalRefunded = 0,
                    Currency = "USD",
                    LastUpdated = DateTime.UtcNow
                };
            }

            return new PaymentStats
            {
                TotalPayments = stats.TotalPayments,
                CompletedPayments = stats.CompletedPayments,
                FailedPayments = stats.FailedPayments,
                PendingPayments = stats.PendingPayments,
                ProcessingPayments = stats.ProcessingPayments,
                RefundedPayments = stats.RefundedPayments,
                TotalRevenue = stats.TotalRevenue,
                TotalRefunded = stats.TotalRefunded,
                Currency = stats.MostCommonCurrency ?? "USD",
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<decimal> GetTotalRevenueByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Where(p => p.ClientId == clientId && EF.Property<string>(p, "Status") == "Completed" && p.Amount != null)
                .SumAsync(p => p.Amount!.Amount, cancellationToken);
        }

        public async Task<int> GetPaymentsCountByStatusForClientAsync(PaymentStatus status, Guid clientId, CancellationToken cancellationToken = default)
        {
            // IMPORTANT: Convert enum to string for EF Core ValueConverter compatibility
            var statusString = status.ToString();
            return await _context.Payments.CountAsync(p => EF.Property<string>(p, "Status") == statusString && p.ClientId == clientId, cancellationToken);
        }

        // Rate limiting operations
        public async Task<TimeSpan?> GetRateLimitDelayAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var limit = _securityLimitService.MaxPaymentAttemptsPerWindow;
            var timeWindow = _securityLimitService.PaymentAttemptWindow;
            var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);

            // OPTIMIZED: Single query with projection to get count and oldest timestamp
            var rateLimitInfo = await _context.Payments
                .Where(p => p.ClientId == clientId && p.CreatedAt >= cutoffTime)
                .OrderBy(p => p.CreatedAt)
                .Take(limit)
                .Select(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            if (rateLimitInfo.Count >= limit)
            {
                var oldestAttempt = rateLimitInfo.First();
                var delay = timeWindow - (DateTime.UtcNow - oldestAttempt);
                return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
            }

            return null;
        }

        /// <summary>
        /// Records payment attempt for rate limiting
        /// NOTE: Current implementation uses Payment.CreatedAt for rate limiting.
        /// If more precise tracking is needed, create a separate PaymentAttempts table.
        /// </summary>
        public async Task RecordPaymentAttemptAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            // IMPLEMENTATION NOTE: Rate limiting currently relies on Payment.CreatedAt timestamps.
            // This is sufficient for basic rate limiting but has limitations:
            // 1. Only tracks actual payment creation, not all attempts
            // 2. Cannot track failed attempts that don't create Payment records
            //
            // For production use, consider implementing a separate PaymentAttempts table:
            // - CREATE TABLE PaymentAttempts (Id, ClientId, AttemptedAt, Success, FailureReason)
            // - Record ALL attempts (success and failure)
            // - Use sliding window algorithm for precise rate limiting

            _logger.LogDebug("RecordPaymentAttemptAsync called for client {ClientId}. Using Payment.CreatedAt for tracking.",
                clientId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Records multiple payment attempts for rate limiting - batch operation
        /// NOTE: See RecordPaymentAttemptAsync for implementation details
        /// </summary>
        public async Task RecordPaymentAttemptsAsync(Guid clientId, int count, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("RecordPaymentAttemptsAsync called for client {ClientId} with count {Count}",
                clientId, count);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets queryable failed payments for retry logic
        /// </summary>
        public async Task<IQueryable<Payment>> GetFailedPaymentsQueryAsync(Guid? clientId = null, CancellationToken cancellationToken = default)
        {
            // IMPORTANT: Use EF.Property<string> for enum comparisons to avoid InvalidCastException
            var query = _context.Payments
                .Where(p => EF.Property<string>(p, "Status") == "Failed")
                .AsQueryable();

            if (clientId.HasValue)
            {
                query = query.Where(p => p.ClientId == clientId.Value);
            }

            return await Task.FromResult(query);
        }

        /// <summary>
        /// Gets payments for metrics and analytics with proper security filtering
        /// SECURITY: Filters by tenantId at SQL level, includes navigation properties for efficiency
        /// PERFORMANCE: Uses IQueryable to allow further filtering, includes related entities to prevent N+1
        /// </summary>
        public async Task<IQueryable<Payment>> GetPaymentsForMetricsAsync(
            Guid tenantId,
            DateTime startDate,
            DateTime endDate,
            Guid? providerId = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetPaymentsForMetricsAsync called for tenant {TenantId}, period {StartDate} to {EndDate}, provider {ProviderId}",
                tenantId, startDate, endDate, providerId);

            // Build query with all necessary includes to prevent N+1
            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                        .ThenInclude(pl => pl.Provider)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId &&
                           p.CreatedAt >= startDate &&
                           p.CreatedAt < endDate.AddDays(1)); // Include full end day

            // Apply provider filter if specified
            if (providerId.HasValue)
            {
                query = query.Where(p => p.Subscription.Plan.Provider.Id == providerId.Value);
            }

            return await Task.FromResult(query);
        }

        /// <summary>
        /// Gets multiple payments by IDs for a specific client (batch operation)
        /// SECURITY: Verifies both TenantId and ClientId to prevent cross-tenant data access
        /// </summary>
        public async Task<Dictionary<Guid, Payment>> GetByIdsForClientAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default)
        {
            if (paymentIds == null || paymentIds.Count == 0)
                return new Dictionary<Guid, Payment>();

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("SECURITY: {MethodName} called without tenant context for {Count} payments",
                    nameof(GetByIdsForClientAsync), paymentIds.Count);
                return new Dictionary<Guid, Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.TenantId == tenantId && // SECURITY: Filter by tenant first
                            p.ClientId == clientId &&
                            paymentIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            return payments.ToDictionary(p => p.Id, p => p);
        }

        /// <summary>
        /// Gets payments for metrics calculations with secure filtering
        /// SECURITY: Filters by TenantId, date range, and optional ProviderId
        /// PERFORMANCE: Optimized query with proper includes for metrics calculations
        /// </summary>
        public async Task<IQueryable<Payment>> GetPaymentsForMetricsAsync(TenantId tenantId, DateTime startDate, DateTime endDate, Guid? providerId, CancellationToken cancellationToken = default)
        {
            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("SECURITY: {MethodName} called without tenant context", nameof(GetPaymentsForMetricsAsync));
                return _context.Payments.Where(p => false); // Return empty query
            }

            // SECURITY: Ensure we're using the correct tenant
            if (_tenantContext.CurrentTenantId != tenantId)
            {
                _logger.LogWarning("SECURITY: {MethodName} called with mismatched tenant context. Expected: {ExpectedTenant}, Actual: {ActualTenant}",
                    nameof(GetPaymentsForMetricsAsync), tenantId, _tenantContext.CurrentTenantId);
                return _context.Payments.Where(p => false); // Return empty query
            }

            var query = _context.Payments
                .AsNoTracking() // Performance: No tracking needed for metrics
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Plan)
                        .ThenInclude(plan => plan.Provider)
                .Where(p => p.TenantId == tenantId && // SECURITY: Filter by tenant
                           p.CreatedAt.Date >= startDate.Date &&
                           p.CreatedAt.Date <= endDate.Date);

            // Apply provider filter if specified
            if (providerId.HasValue)
            {
                query = query.Where(p => p.Subscription.Plan.Provider.Id == providerId.Value);
            }

            return await Task.FromResult(query);
        }

        /// <summary>
        /// Gets pending payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Payment>> GetPendingPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetPendingPaymentsForTenantAsync called for tenant {TenantId}", tenantId.Value);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Pending" && p.TenantId == tenantId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets failed payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Payment>> GetFailedPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetFailedPaymentsForTenantAsync called for tenant {TenantId}", tenantId.Value);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Failed" && p.TenantId == tenantId)
                .OrderByDescending(p => p.FailedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets processing payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Payment>> GetProcessingPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetProcessingPaymentsForTenantAsync called for tenant {TenantId}", tenantId.Value);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => EF.Property<string>(p, "Status") == "Processing" && p.TenantId == tenantId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets payments with external IDs for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        public async Task<IEnumerable<Payment>> GetPaymentsWithExternalIdForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("GetPaymentsWithExternalIdForTenantAsync called for tenant {TenantId}", tenantId.Value);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => !string.IsNullOrEmpty(p.ExternalPaymentId) && p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets payment by ID without tenant validation (WEBHOOK/ADMIN ONLY)
        /// WARNING: This method bypasses tenant validation. Only use in webhook handlers
        /// that have been verified by signature middleware or admin operations.
        /// CALLER MUST verify tenant after retrieving the payment.
        /// </summary>
        public async Task<Payment?> GetByIdUnsafeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SECURITY: GetByIdUnsafeAsync called for payment {PaymentId}. Ensure this is from a verified webhook or admin operation.", id);

            return await _context.Payments
                .IgnoreQueryFilters() // Bypass tenant filter
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Gets payment by external payment ID without tenant validation (WEBHOOK/ADMIN ONLY)
        /// WARNING: This method bypasses tenant validation. Only use in webhook handlers
        /// that have been verified by signature middleware (e.g., Stripe webhooks).
        /// CALLER MUST verify tenant after retrieving the payment.
        /// </summary>
        public async Task<Payment?> GetByExternalPaymentIdUnsafeAsync(string externalPaymentId, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("SECURITY: GetByExternalPaymentIdUnsafeAsync called for external payment {ExternalPaymentId}. Ensure this is from a verified webhook.", externalPaymentId);

            return await _context.Payments
                .IgnoreQueryFilters() // Bypass tenant filter
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
        }

        private static (int pageNumber, int pageSize) ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }
    }
}
