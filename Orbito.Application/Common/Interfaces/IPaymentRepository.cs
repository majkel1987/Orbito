using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Common.Interfaces
{
    public interface IPaymentRepository
    {
        // Read operations
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByClientIdAsync(Guid clientId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default);

        // Create operations
        Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);

        // Update operations
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

        // Delete operations
        Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default);

        // Validation operations
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string externalTransactionId, CancellationToken cancellationToken = default);

        // Stats operations
        Task<PaymentStats> GetPaymentStatsAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
        Task<int> GetPaymentsCountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
        Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    }

    public record PaymentStats
    {
        public int TotalPayments { get; init; }
        public int CompletedPayments { get; init; }
        public int FailedPayments { get; init; }
        public int PendingPayments { get; init; }
        public int ProcessingPayments { get; init; }
        public int RefundedPayments { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal TotalRefunded { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime LastUpdated { get; init; }
    }
}
