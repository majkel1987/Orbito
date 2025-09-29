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

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalTransactionId))
                throw new ArgumentException("External transaction ID cannot be null or empty", nameof(externalTransactionId));

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId, cancellationToken);
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

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Subscription)
                .Include(p => p.Client)
                .Where(p => p.Status == PaymentStatus.Failed)
                .OrderByDescending(p => p.FailedAt)
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

        public async Task<PaymentStats> GetPaymentStatsAsync(CancellationToken cancellationToken = default)
        {
            // Single query to get all stats at once
            var stats = await _context.Payments
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

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.Amount != null)
                .SumAsync(p => p.Amount!.Amount, cancellationToken);
        }

        public async Task<int> GetPaymentsCountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Payments.CountAsync(p => p.Status == status, cancellationToken);
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

        private static (int pageNumber, int pageSize) ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }
    }
}
