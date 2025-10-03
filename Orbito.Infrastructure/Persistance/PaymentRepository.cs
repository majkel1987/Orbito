using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;

        public PaymentRepository(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use GetByIdForClientAsync instead.")]
        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            Console.WriteLine($"[SECURITY WARNING] Deprecated GetByIdAsync called for payment {id}. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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
            Console.WriteLine($"[SECURITY WARNING] Deprecated GetByExternalTransactionIdAsync called for transaction {externalTransactionId}. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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
            Console.WriteLine($"[SECURITY WARNING] Deprecated GetByExternalPaymentIdAsync called for payment {externalPaymentId}. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.SubscriptionId == subscriptionId)
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
            Console.WriteLine($"[SECURITY WARNING] Deprecated GetByStatusAsync called for status {status}. This method should not be used!");

            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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
            Console.WriteLine("[SECURITY WARNING] ADMIN-ONLY GetPendingPaymentsAsync called. This returns data from ALL tenants!");

            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == PaymentStatus.Pending && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("ADMIN-ONLY: Returns failed payments from ALL clients. Use client-specific methods for regular operations.")]
        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[SECURITY WARNING] ADMIN-ONLY GetFailedPaymentsAsync called. This returns data from ALL tenants!");

            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == PaymentStatus.Failed && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderByDescending(p => p.FailedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns processing payments from ALL tenants without verification. Use tenant-specific methods instead.")]
        public async Task<IEnumerable<Payment>> GetProcessingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            Console.WriteLine("[SECURITY WARNING] Deprecated GetProcessingPaymentsAsync called. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
                return new List<Payment>();
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == PaymentStatus.Processing && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payments with external IDs from ALL tenants without verification. Use tenant-specific methods instead.")]
        public async Task<IEnumerable<Payment>> GetPaymentsWithExternalIdAsync(CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            Console.WriteLine("[SECURITY WARNING] Deprecated GetPaymentsWithExternalIdAsync called. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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
            Console.WriteLine("[SECURITY WARNING] Deprecated GetPaymentStatsAsync called. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
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
            var stats = await _context.Payments
                .Where(p => p.TenantId == tenantId) // SECURITY: Filter by tenant
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPayments = g.Count(),
                    CompletedPayments = g.Count(p => p.Status == PaymentStatus.Completed),
                    FailedPayments = g.Count(p => p.Status == PaymentStatus.Failed),
                    PendingPayments = g.Count(p => p.Status == PaymentStatus.Pending),
                    ProcessingPayments = g.Count(p => p.Status == PaymentStatus.Processing),
                    RefundedPayments = g.Count(p => p.Status == PaymentStatus.Refunded),
                    TotalRevenue = g.Where(p => p.Status == PaymentStatus.Completed && p.Amount != null)
                                   .Sum(p => (decimal?)p.Amount!.Amount) ?? 0,
                    TotalRefunded = g.Where(p => (p.Status == PaymentStatus.Refunded ||
                                                 p.Status == PaymentStatus.PartiallyRefunded) &&
                                                 p.Amount != null)
                                    .Sum(p => (decimal?)p.Amount!.Amount) ?? 0,
                    MostCommonCurrency = g.Where(p => p.Status == PaymentStatus.Completed && p.Amount != null)
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
            Console.WriteLine("[SECURITY WARNING] Deprecated GetTotalRevenueAsync called. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.Amount != null && p.TenantId == tenantId) // SECURITY: Filter by tenant
                .SumAsync(p => p.Amount!.Amount, cancellationToken);
        }

        [Obsolete("SECURITY RISK: This method returns payment counts from ALL tenants without verification. Use GetPaymentsCountByStatusForClientAsync instead.")]
        public async Task<int> GetPaymentsCountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        {
            // SECURITY: Log deprecated method usage for monitoring
            Console.WriteLine($"[SECURITY WARNING] Deprecated GetPaymentsCountByStatusAsync called for status {status}. This method should not be used!");

            // SECURITY: Apply tenant filtering even for deprecated method
            if (!_tenantContext.HasTenant)
            {
                Console.WriteLine("[SECURITY ERROR] No tenant context available - access denied!");
                return 0;
            }

            var tenantId = _tenantContext.CurrentTenantId;

            return await _context.Payments.CountAsync(p => p.Status == status && p.TenantId == tenantId, cancellationToken); // SECURITY: Filter by tenant
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

        public async Task<decimal> GetTotalRefundedAmountAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            // W tej implementacji zakładamy, że informacja o zwrotach jest przechowywana
            // w polu RefundedAmount lub w oddzielnej tabeli refunds
            // Dla uproszczenia, zwrócimy 0 - należy to dostosować do rzeczywistej struktury danych

            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null)
                return 0;

            // Jeśli mamy pole RefundedAmount w encji Payment
            // return payment.RefundedAmount ?? 0;

            // Jeśli mamy oddzielną tabelę Refunds
            // return await _context.Refunds
            //     .Where(r => r.PaymentId == paymentId && r.Status == RefundStatus.Completed)
            //     .SumAsync(r => r.Amount.Amount, cancellationToken);

            // Tymczasowe rozwiązanie - zwróć pełną kwotę jeśli częściowo lub całkowicie zwrócone
            if (payment.Status == PaymentStatus.Refunded)
                return payment.Amount?.Amount ?? 0;

            if (payment.Status == PaymentStatus.PartiallyRefunded)
            {
                // W rzeczywistości powinniśmy śledzić dokładną kwotę zwróconą
                // Na razie zwrócimy połowę kwoty jako przykład
                return (payment.Amount?.Amount ?? 0) * 0.5m;
            }

            return 0;
        }

        // Security-enhanced methods with client verification
        public async Task<Payment?> GetByIdForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
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
            var stats = await _context.Payments
                .Where(p => p.ClientId == clientId)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalPayments = g.Count(),
                    CompletedPayments = g.Count(p => p.Status == PaymentStatus.Completed),
                    FailedPayments = g.Count(p => p.Status == PaymentStatus.Failed),
                    PendingPayments = g.Count(p => p.Status == PaymentStatus.Pending),
                    ProcessingPayments = g.Count(p => p.Status == PaymentStatus.Processing),
                    RefundedPayments = g.Count(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded),
                    TotalRevenue = g.Where(p => p.Status == PaymentStatus.Completed && p.Amount != null).Sum(p => p.Amount!.Amount),
                    TotalRefunded = g.Where(p => (p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded) && p.Amount != null).Sum(p => p.Amount!.Amount),
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
                .Where(p => p.ClientId == clientId && p.Status == PaymentStatus.Completed && p.Amount != null)
                .SumAsync(p => p.Amount!.Amount, cancellationToken);
        }

        public async Task<int> GetPaymentsCountByStatusForClientAsync(PaymentStatus status, Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.CountAsync(p => p.Status == status && p.ClientId == clientId, cancellationToken);
        }

        // Rate limiting operations
        public async Task<TimeSpan?> GetRateLimitDelayAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var recentAttemptsLimit = 5;
            var timeWindow = TimeSpan.FromMinutes(15);
            var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);

            var recentAttempts = await _context.Payments
                .Where(p => p.ClientId == clientId && p.CreatedAt >= cutoffTime)
                .CountAsync(cancellationToken);

            if (recentAttempts >= recentAttemptsLimit)
            {
                var oldestRecentAttempt = await _context.Payments
                    .Where(p => p.ClientId == clientId && p.CreatedAt >= cutoffTime)
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => p.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (oldestRecentAttempt != default)
                {
                    var delay = timeWindow - (DateTime.UtcNow - oldestRecentAttempt);
                    return delay > TimeSpan.Zero ? delay : TimeSpan.FromMinutes(1);
                }
            }

            return null;
        }

        public async Task RecordPaymentAttemptAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            // This method would typically create a record in a separate rate limiting table
            // For now, we'll just ensure it's implemented to satisfy the interface
            await Task.CompletedTask;
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
