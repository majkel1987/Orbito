using Orbito.Application.DTOs;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers;

/// <summary>
/// Centralized mapper for Provider entity to ProviderDto conversion.
/// Eliminates code duplication across multiple handlers.
/// </summary>
public static class ProviderMapper
{
    /// <summary>
    /// Maps a Provider entity to ProviderDto.
    /// </summary>
    /// <param name="provider">The provider entity to map.</param>
    /// <returns>A ProviderDto with all properties populated.</returns>
    /// <remarks>
    /// WARNING: This mapper accesses navigation properties (Plans, Subscriptions, User).
    /// Ensure the following are eager-loaded to avoid lazy loading exceptions or N+1 queries:
    /// <code>
    /// .Include(p => p.Plans)
    /// .Include(p => p.Subscriptions)
    /// .Include(p => p.User)
    /// </code>
    /// </remarks>
    public static ProviderDto ToDto(Provider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        return new ProviderDto
        {
            Id = provider.Id,
            TenantId = provider.TenantId.Value,
            UserId = provider.UserId,
            BusinessName = provider.BusinessName,
            Description = provider.Description,
            Avatar = provider.Avatar,
            SubdomainSlug = provider.SubdomainSlug,
            CustomDomain = provider.CustomDomain,
            IsActive = provider.IsActive,
            CreatedAt = provider.CreatedAt,
            MonthlyRevenue = provider.MonthlyRevenue.Amount,
            Currency = provider.MonthlyRevenue.Currency,
            ActiveClientsCount = provider.ActiveClientsCount,
            PlansCount = provider.Plans.Count,
            SubscriptionsCount = provider.Subscriptions.Count,
            UserEmail = provider.User?.Email,
            UserFirstName = provider.User?.FirstName,
            UserLastName = provider.User?.LastName
        };
    }

    /// <summary>
    /// Maps a collection of Provider entities to ProviderDto list.
    /// </summary>
    /// <param name="providers">The providers to map.</param>
    /// <returns>A list of ProviderDto objects.</returns>
    public static IReadOnlyList<ProviderDto> ToDtos(IEnumerable<Provider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        return providers.Select(ToDto).ToList();
    }
}
