using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Payment entity operations.
/// Provides secure CRUD and query operations with tenant isolation.
/// </summary>
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

        // SECURE: Provider dashboard - list all payments for tenant with filtering
        Task<IEnumerable<Payment>> GetAllForTenantAsync(
            TenantId tenantId,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            PaymentStatus? status = null,
            Guid? clientId = null,
            CancellationToken cancellationToken = default);
        Task<int> GetCountForTenantAsync(
            TenantId tenantId,
            string? searchTerm = null,
            PaymentStatus? status = null,
            Guid? clientId = null,
            CancellationToken cancellationToken = default);
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

        // Query operations for retry logic
        Task<IQueryable<Payment>> GetFailedPaymentsQueryAsync(Guid? clientId = null, CancellationToken cancellationToken = default);

        // SECURE: Metrics operations with tenant filtering
        Task<IQueryable<Payment>> GetPaymentsForMetricsAsync(
            Guid tenantId,
            DateTime startDate,
            DateTime endDate,
            Guid? providerId = null,
            CancellationToken cancellationToken = default);

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
        Task RecordPaymentAttemptsAsync(Guid clientId, int count, CancellationToken cancellationToken = default);

        // Batch operations
        Task<Dictionary<Guid, Payment>> GetByIdsForClientAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default);


        // BACKGROUND JOB METHODS: Explicit TenantId for multi-tenant operations
        /// <summary>
        /// Gets pending payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Payment>> GetPendingPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets failed payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Payment>> GetFailedPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets processing payments for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Payment>> GetProcessingPaymentsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payments with external IDs for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        Task<IEnumerable<Payment>> GetPaymentsWithExternalIdForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        // WEBHOOK HANDLER METHODS: Bypass tenant validation for verified external requests
        /// <summary>
        /// Gets payment by ID without tenant validation (WEBHOOK/ADMIN ONLY)
        /// WARNING: This method bypasses tenant validation. Only use in webhook handlers
        /// that have been verified by signature middleware or admin operations.
        /// CALLER MUST verify tenant after retrieving the payment.
        /// </summary>
        Task<Payment?> GetByIdUnsafeAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payment by external payment ID without tenant validation (WEBHOOK/ADMIN ONLY)
        /// WARNING: This method bypasses tenant validation. Only use in webhook handlers
        /// that have been verified by signature middleware (e.g., Stripe webhooks).
        /// CALLER MUST verify tenant after retrieving the payment.
        /// </summary>
    Task<Payment?> GetByExternalPaymentIdUnsafeAsync(string externalPaymentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents aggregated payment statistics.
/// </summary>
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
