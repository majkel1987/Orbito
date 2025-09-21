using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    public interface IProviderService
    {
        Task<bool> ValidateSubdomainAsync(string subdomainSlug, Guid? excludeProviderId = null, CancellationToken cancellationToken = default);
        Task<bool> CanProviderBeDeletedAsync(Guid providerId, CancellationToken cancellationToken = default);
        Task<Provider?> GetProviderWithMetricsAsync(Guid providerId, CancellationToken cancellationToken = default);
        Task UpdateProviderMetricsAsync(Guid providerId, CancellationToken cancellationToken = default);
        Task<bool> IsProviderActiveAsync(Guid providerId, CancellationToken cancellationToken = default);
    }
}
