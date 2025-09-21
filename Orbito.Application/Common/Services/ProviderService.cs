using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly ILogger<ProviderService> _logger;

        public ProviderService(
            IProviderRepository providerRepository,
            ILogger<ProviderService> logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        public async Task<bool> ValidateSubdomainAsync(string subdomainSlug, Guid? excludeProviderId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subdomainSlug))
                {
                    _logger.LogWarning("Subdomain validation failed: empty subdomain");
                    return false;
                }

                // Check for reserved subdomains
                var reservedSubdomains = new[] { "admin", "api", "www", "app", "dashboard", "support", "help", "docs" };
                if (reservedSubdomains.Contains(subdomainSlug.ToLowerInvariant()))
                {
                    _logger.LogWarning("Subdomain validation failed: reserved subdomain {Subdomain}", subdomainSlug);
                    return false;
                }

                // Check if subdomain is available
                var isAvailable = await _providerRepository.IsSubdomainAvailableAsync(subdomainSlug, excludeProviderId, cancellationToken);
                
                if (!isAvailable)
                {
                    _logger.LogWarning("Subdomain validation failed: already taken {Subdomain}", subdomainSlug);
                }

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating subdomain: {Subdomain}", subdomainSlug);
                return false;
            }
        }

        public async Task<bool> CanProviderBeDeletedAsync(Guid providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId, cancellationToken);
                if (provider == null)
                {
                    _logger.LogWarning("Provider not found for deletion check: {ProviderId}", providerId);
                    return false;
                }

                var canBeDeleted = provider.CanBeDeleted();
                
                _logger.LogInformation("Provider deletion check: {ProviderId}, CanDelete: {CanDelete}", 
                    providerId, canBeDeleted);

                return canBeDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if provider can be deleted: {ProviderId}", providerId);
                return false;
            }
        }

        public async Task<Provider?> GetProviderWithMetricsAsync(Guid providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId, cancellationToken);
                if (provider == null)
                {
                    _logger.LogWarning("Provider not found for metrics: {ProviderId}", providerId);
                    return null;
                }

                // Update metrics if needed
                await UpdateProviderMetricsAsync(providerId, cancellationToken);

                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider with metrics: {ProviderId}", providerId);
                return null;
            }
        }

        public async Task UpdateProviderMetricsAsync(Guid providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId, cancellationToken);
                if (provider == null)
                {
                    _logger.LogWarning("Provider not found for metrics update: {ProviderId}", providerId);
                    return;
                }

                // Update active clients count
                var activeClientsCount = provider.Clients.Count(c => c.IsActive);
                provider.UpdateActiveClientsCount(activeClientsCount);

                // Calculate monthly revenue (this would typically involve more complex logic)
                // For now, we'll use a simple calculation based on active subscriptions
                var monthlyRevenue = provider.Subscriptions
                    .Where(s => s.Status == Domain.Enums.SubscriptionStatus.Active)
                    .Sum(s => s.CurrentPrice.Amount);

                provider.UpdateMonthlyRevenue(Money.Create(monthlyRevenue, provider.MonthlyRevenue.Currency));

                await _providerRepository.UpdateAsync(provider, cancellationToken);

                _logger.LogInformation("Provider metrics updated: {ProviderId}, ActiveClients: {ActiveClients}, Revenue: {Revenue}",
                    providerId, activeClientsCount, monthlyRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider metrics: {ProviderId}", providerId);
            }
        }

        public async Task<bool> IsProviderActiveAsync(Guid providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId, cancellationToken);
                return provider?.IsActive ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if provider is active: {ProviderId}", providerId);
                return false;
            }
        }
    }
}
