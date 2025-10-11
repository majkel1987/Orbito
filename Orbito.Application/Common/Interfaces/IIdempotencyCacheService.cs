namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service for managing idempotency cache operations
/// </summary>
public interface IIdempotencyCacheService
{
    /// <summary>
    /// Tries to get a cached response for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached response if found, null otherwise</returns>
    Task<IdempotencyCacheEntry?> TryGetCachedResponseAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches a response for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="response">The response to cache</param>
    /// <param name="ttl">Time to live for the cache entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CacheResponseAsync(string key, IdempotencyCacheEntry response, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached response for the given idempotency key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveCachedResponseAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a distributed lock can be acquired for the given key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="timeout">Lock timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if lock was acquired, false otherwise</returns>
    Task<bool> TryAcquireLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a distributed lock for the given key
    /// </summary>
    /// <param name="key">The idempotency key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ReleaseLockAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired cache entries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a cached idempotency response
/// </summary>
public class IdempotencyCacheEntry
{
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The response headers
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// The response body
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// The content type of the response
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// When this entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The tenant ID associated with this entry
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The client ID associated with this entry
    /// </summary>
    public Guid? ClientId { get; set; }
}
