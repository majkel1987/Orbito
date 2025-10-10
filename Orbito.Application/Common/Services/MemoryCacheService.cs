using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Common.Services;

/// <summary>
/// Memory-based cache service implementation
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return Task.FromResult<T?>(default);
            }

            if (_memoryCache.TryGetValue<T>(key, out var result))
            {
                _logger.LogDebug("Cache HIT for key {Key}", key);
                return Task.FromResult<T?>(result);
            }
            
            _logger.LogDebug("Cache MISS for key {Key}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return Task.CompletedTask;
            }

            if (value == null || (value is ValueType && EqualityComparer<T>.Default.Equals(value, default)))
            {
                _logger.LogWarning("Cannot cache null or default value for key {Key}", key);
                return Task.CompletedTask;
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Cached value for key {Key} with TTL {Ttl}", key, ttl);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return Task.CompletedTask;
            }

            _memoryCache.Remove(key);
            _logger.LogDebug("Removed value from cache for key {Key}", key);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key {Key}", key);
            return Task.CompletedTask;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Cache key is null or empty");
                return await factory();
            }

            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
            {
                return cachedValue;
            }

            // Create new value using factory
            var newValue = await factory();
            if (newValue != null && !EqualityComparer<T>.Default.Equals(newValue, default))
            {
                await SetAsync(key, newValue, ttl, cancellationToken);
            }

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSetAsync for key {Key}", key);
            // Fallback to factory if cache fails
            return await factory();
        }
    }
}
