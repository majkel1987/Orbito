using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    public interface IProviderRepository
    {
        Task<Provider?> GetBySubdomainSlugAsync(string subdomainSlug, CancellationToken cancellationToken = default);
        Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string subdomainSlug, CancellationToken cancellationToken = default);
    }
}
