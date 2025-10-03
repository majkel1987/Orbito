using System;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Common.Interfaces
{
    public interface IPaymentRepository
    {
        // DEPRECATED: Read operations without client verification - SECURITY RISK!
        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use GetByIdForClientAsync instead.")]
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use GetByExternalTransactionIdForClientAsync instead.")]
        Task<Payment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken cancellationToken = default);

        [Obsolete("SECURITY RISK: This method allows access to any payment without client verification. Use client-specific methods instead.")]
        Task<Payment?> GetByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);

        [Obsolete("SECURITY RISK: This method returns payments from ALL clients without verification. Use client-specific methods instead.")]
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        // SECURE: Read operations with client verification
        Task<Payment?> GetByIdForClientAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default);
        Task<Payment?> GetByExternalTransactionIdForClientAsync(string externalTransactionId, Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByClientIdAsync(Guid clientId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        // ADMIN-ONLY: These methods return data across ALL tenants - use with extreme caution
        [Obsolete("ADMIN-ONLY: Returns pending payments from ALL clients. Use client-specific methods for regular operations.")]
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);

        [Obsolete("ADMIN-ONLY: Returns failed payments from ALL clients. Use client-specific methods for regular operations.")]
        Task<IEnumerable<Payment>> GetFailedPaymentsAsync(CancellationToken cancellationToken = default);

        [Obsolete("ADMIN-ONLY: Returns processing payments from ALL clients. Use client-specific methods for regular operations.")]
        Task<IEnumerable<Payment>> GetProcessingPaymentsAsync(CancellationToken cancellationToken = default);

        [Obsolete("ADMIN-ONLY: Returns payments with external ID from ALL clients. Use client-specific methods for regular operations.")]
        Task<IEnumerable<Payment>> GetPaymentsWithExternalIdAsync(CancellationToken cancellationToken = default);

        // Create operations
        Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);

        // Update operations
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

        // Delete operations
        Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default);

        // Validation operations
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string externalTransactionId, CancellationToken cancellationToken = default);

        // Security operations
        Task<Payment?> GetRecentBySubscriptionIdAsync(Guid subscriptionId, TimeSpan timeWindow, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRefundedAmountAsync(Guid paymentId, CancellationToken cancellationToken = default);

        // Stats operations - SECURE (client-specific)
        Task<PaymentStats> GetPaymentStatsByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalRevenueByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<int> GetPaymentsCountByStatusForClientAsync(PaymentStatus status, Guid clientId, CancellationToken cancellationToken = default);
        Task<int> GetCountBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

        // ADMIN-ONLY: Stats operations across ALL tenants - use with extreme caution
        [Obsolete("ADMIN-ONLY: Returns stats from ALL clients. Use GetPaymentStatsByClientAsync for regular operations.")]
        Task<PaymentStats> GetPaymentStatsAsync(CancellationToken cancellationToken = default);

        [Obsolete("ADMIN-ONLY: Returns total revenue from ALL clients. Use GetTotalRevenueByClientAsync for regular operations.")]
        Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);

        [Obsolete("ADMIN-ONLY: Returns payment count from ALL clients. Use GetPaymentsCountByStatusForClientAsync for regular operations.")]
        Task<int> GetPaymentsCountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);

        // Rate limiting operations
        Task<TimeSpan?> GetRateLimitDelayAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task RecordPaymentAttemptAsync(Guid clientId, CancellationToken cancellationToken = default);
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
