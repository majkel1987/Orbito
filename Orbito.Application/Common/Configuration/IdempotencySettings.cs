namespace Orbito.Application.Common.Configuration;

/// <summary>
/// Configuration settings for idempotency functionality
/// </summary>
public class IdempotencySettings
{
    /// <summary>
    /// Cache TTL for idempotency keys in hours
    /// </summary>
    public int CacheTtlHours { get; set; } = 24;

    /// <summary>
    /// Redis connection string for idempotency cache
    /// </summary>
    public string RedisConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable idempotency middleware
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum length of idempotency key in characters
    /// </summary>
    public int MaxKeyLength { get; set; } = 100;

    /// <summary>
    /// Minimum length of idempotency key in characters
    /// </summary>
    public int MinKeyLength { get; set; } = 16;

    /// <summary>
    /// Whether to require idempotency key for payment creation
    /// </summary>
    public bool RequireIdempotencyKey { get; set; } = true;

    /// <summary>
    /// Distributed lock timeout in seconds
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use distributed locking for thread safety
    /// </summary>
    public bool UseDistributedLock { get; set; } = true;

    /// <summary>
    /// Cache key prefix for idempotency entries
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "idempotency:";

    /// <summary>
    /// Whether to log idempotency operations
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Maximum number of cached responses per tenant
    /// </summary>
    public int MaxCachedResponsesPerTenant { get; set; } = 1000;

    /// <summary>
    /// Whether to enable automatic cleanup of expired cache entries
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// Cleanup interval in hours
    /// </summary>
    public int CleanupIntervalHours { get; set; } = 6;
}
