using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Orbito.Infrastructure.Services;

/// <summary>
/// In-memory implementation of idempotency cache service
/// In production, this should be replaced with Redis implementation
/// </summary>
public class IdempotencyCacheService : IIdempotencyCacheService
{
    private readonly ILogger<IdempotencyCacheService> _logger;
    private readonly IdempotencySettings _settings;
    private readonly ConcurrentDictionary<string, IdempotencyCacheEntry> _cache = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly Timer? _cleanupTimer;

    public IdempotencyCacheService(
        ILogger<IdempotencyCacheService> logger,
        IOptions<IdempotencySettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        // Start cleanup timer if auto cleanup is enabled
        if (_settings.EnableAutoCleanup)
        {
            var cleanupInterval = TimeSpan.FromHours(_settings.CleanupIntervalHours);
            _cleanupTimer = new Timer(PerformCleanup, null, cleanupInterval, cleanupInterval);
        }
    }

    public Task<IdempotencyCacheEntry?> TryGetCachedResponseAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to get cached response with null or empty key");
            return Task.FromResult<IdempotencyCacheEntry?>(null); // FIXED: Return Task<null> instead of null
        }

        try
        {
            var cacheKey = GetCacheKey(key);
            
            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                // Check if entry is expired
                var expirationTime = entry.CreatedAt.AddHours(_settings.CacheTtlHours);
                if (DateTime.UtcNow > expirationTime)
                {
                    _logger.LogDebug("Cached entry for key {Key} has expired, removing", key);
                    _cache.TryRemove(cacheKey, out _);
                    return Task.FromResult<IdempotencyCacheEntry?>(null);
                }

                _logger.LogDebug("Found cached response for key {Key}", key);
                return Task.FromResult<IdempotencyCacheEntry?>(entry);
            }

            _logger.LogDebug("No cached response found for key {Key}", key);
            return Task.FromResult<IdempotencyCacheEntry?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached response for key {Key}", key);
            return Task.FromResult<IdempotencyCacheEntry?>(null);
        }
    }

    public Task CacheResponseAsync(string key, IdempotencyCacheEntry response, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to cache response with null or empty key");
            return Task.CompletedTask;
        }

        if (response == null)
        {
            _logger.LogWarning("Attempted to cache null response for key {Key}", key);
            return Task.CompletedTask;
        }

        try
        {
            var cacheKey = GetCacheKey(key);
            
            // Use the provided TTL or default from settings
            var effectiveTtl = ttl != TimeSpan.Zero ? ttl : TimeSpan.FromHours(_settings.CacheTtlHours);
            
            // Create a copy of the response with current timestamp
            var entryToCache = new IdempotencyCacheEntry
            {
                StatusCode = response.StatusCode,
                Headers = new Dictionary<string, string>(response.Headers),
                Body = response.Body,
                ContentType = response.ContentType,
                CreatedAt = DateTime.UtcNow,
                TenantId = response.TenantId,
                ClientId = response.ClientId
            };

            _cache.AddOrUpdate(cacheKey, entryToCache, (k, v) => entryToCache);
            
            _logger.LogDebug("Cached response for key {Key} with TTL {Ttl}", key, effectiveTtl);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching response for key {Key}", key);
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveCachedResponseAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to remove cached response with null or empty key");
            return Task.CompletedTask;
        }

        try
        {
            var cacheKey = GetCacheKey(key);
            if (_cache.TryRemove(cacheKey, out _))
            {
                _logger.LogDebug("Removed cached response for key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached response for key {Key}", key);
        }
        
        return Task.CompletedTask;
    }

    public async Task<bool> TryAcquireLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to acquire lock with null or empty key");
            return false;
        }

        try
        {
            var lockKey = GetLockKey(key);
            var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            // FIXED: Use await instead of .Result to prevent deadlock and thread blocking
            var acquired = await semaphore.WaitAsync(timeout, cancellationToken);
            if (acquired)
            {
                _logger.LogDebug("Acquired distributed lock for key {Key}", key);
            }
            else
            {
                _logger.LogDebug("Failed to acquire distributed lock for key {Key} within timeout {Timeout}", key, timeout);
            }

            return acquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for key {Key}", key);
            return false;
        }
    }

    public Task ReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Attempted to release lock with null or empty key");
            return Task.CompletedTask;
        }

        try
        {
            var lockKey = GetLockKey(key);
            if (_locks.TryGetValue(lockKey, out var semaphore))
            {
                semaphore.Release();
                _logger.LogDebug("Released distributed lock for key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for key {Key}", key);
        }
        
        return Task.CompletedTask;
    }

    public Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredKeys = new List<string>();
            var cutoffTime = DateTime.UtcNow.AddHours(-_settings.CacheTtlHours);

            foreach (var kvp in _cache)
            {
                if (kvp.Value.CreatedAt < cutoffTime)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired idempotency cache entries", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during idempotency cache cleanup");
        }
        
        return Task.CompletedTask;
    }

    private string GetCacheKey(string key)
    {
        return $"{_settings.CacheKeyPrefix}response:{key}";
    }

    private string GetLockKey(string key)
    {
        return $"{_settings.CacheKeyPrefix}lock:{key}";
    }

    private void PerformCleanup(object? state)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await CleanupExpiredEntriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled idempotency cache cleanup");
            }
        });
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        
        // Dispose all semaphores
        foreach (var semaphore in _locks.Values)
        {
            semaphore.Dispose();
        }
        _locks.Clear();
    }
}
