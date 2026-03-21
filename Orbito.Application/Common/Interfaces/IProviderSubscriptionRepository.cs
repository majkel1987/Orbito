using Orbito.Domain.Entities;
using Orbito.Domain.Enums;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository for ProviderSubscription entity (Provider's platform subscription to Orbito).
/// </summary>
public interface IProviderSubscriptionRepository
{
    Task<ProviderSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProviderSubscription?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderSubscription>> GetByStatusAsync(ProviderSubscriptionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderSubscription>> GetExpiringTrialsAsync(int daysUntilExpiration, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderSubscription>> GetExpiredTrialsAsync(CancellationToken cancellationToken = default);
    Task<ProviderSubscription> AddAsync(ProviderSubscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProviderSubscription subscription, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
}
