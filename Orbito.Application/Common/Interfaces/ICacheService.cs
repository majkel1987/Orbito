namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service for caching data with TTL support
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    /// <typeparam name="T">Type of the cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value or default if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with TTL
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="ttl">Time to live</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets a value in cache using a factory function
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not cached</param>
    /// <param name="ttl">Time to live</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached or newly created value</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default);
}
