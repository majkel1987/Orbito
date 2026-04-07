using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    public interface IProviderRepository
    {
        // Read operations
        Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Provider?> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
        Task<Provider?> GetBySubdomainSlugAsync(string subdomainSlug, CancellationToken cancellationToken = default);
        Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Provider>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Provider>> GetActiveProvidersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);

        // Create operations
        Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default);

        // Update operations
        Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default);

        // Delete operations
        Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(Provider provider, CancellationToken cancellationToken = default);

        // Validation operations
        Task<bool> ExistsAsync(string subdomainSlug, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> IsSubdomainAvailableAsync(string subdomainSlug, Guid? excludeProviderId = null, CancellationToken cancellationToken = default);

        // Platform Admin operations
        Task<Guid?> GetPlatformAdminTenantIdAsync(CancellationToken cancellationToken = default);
    }
}
