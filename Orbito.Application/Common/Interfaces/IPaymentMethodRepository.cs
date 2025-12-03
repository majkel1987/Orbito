using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Repository interface for payment method operations
    /// </summary>
    public interface IPaymentMethodRepository
    {
        /// <summary>
        /// Gets payment method by ID
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <param name="clientId">Client ID for ownership verification</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment method if found, null otherwise</returns>
        Task<PaymentMethod?> GetByIdAsync(Guid id, Guid clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payment methods by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="type">Filter by payment method type</param>
        /// <param name="activeOnly">Include only active payment methods</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of payment methods</returns>
        Task<IEnumerable<PaymentMethod>> GetByClientIdAsync(
            Guid clientId,
            int pageNumber = 1,
            int pageSize = 10,
            PaymentMethodType? type = null,
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payment methods with count by client ID (optimized single query)
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="type">Filter by payment method type</param>
        /// <param name="activeOnly">Include only active payment methods</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple with payment methods list and total count</returns>
        Task<(IEnumerable<PaymentMethod> PaymentMethods, int TotalCount)> GetByClientIdWithCountAsync(
            Guid clientId,
            int pageNumber = 1,
            int pageSize = 10,
            PaymentMethodType? type = null,
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default payment methods by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of default payment methods</returns>
        Task<IEnumerable<PaymentMethod>> GetDefaultPaymentMethodsByClientAsync(
            Guid clientId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets payment methods by type
        /// </summary>
        /// <param name="type">Payment method type</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of payment methods</returns>
        Task<IEnumerable<PaymentMethod>> GetByTypeAsync(
            PaymentMethodType type, 
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets expired payment methods (admin-only, returns all tenants)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of expired payment methods</returns>
        [Obsolete("ADMIN-ONLY: Returns expired payment methods from ALL tenants. Use GetExpiredPaymentMethodsForTenantAsync for tenant-specific operations.")]
        Task<IEnumerable<PaymentMethod>> GetExpiredPaymentMethodsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets expired payment methods for a specific tenant (for background jobs)
        /// SECURITY: Requires explicit TenantId to prevent cross-tenant access
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of expired payment methods for the tenant</returns>
        Task<IEnumerable<PaymentMethod>> GetExpiredPaymentMethodsForTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets count of payment methods by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="type">Filter by payment method type</param>
        /// <param name="activeOnly">Include only active payment methods</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of payment methods</returns>
        Task<int> GetCountByClientIdAsync(
            Guid clientId, 
            PaymentMethodType? type = null, 
            bool activeOnly = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new payment method
        /// </summary>
        /// <param name="paymentMethod">Payment method to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added payment method</returns>
        Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing payment method
        /// </summary>
        /// <param name="paymentMethod">Payment method to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a payment method
        /// </summary>
        /// <param name="paymentMethod">Payment method to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if client can add more payment methods
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if client can add payment method, false otherwise</returns>
        Task<bool> CanAddPaymentMethodAsync(Guid clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets count of active payment methods by client ID
        /// </summary>
        /// <param name="clientId">Client ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of active payment methods</returns>
        Task<int> GetActiveCountByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    }
}
