using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbito.Application.Common.Configuration;
using Orbito.Application.Common.Interfaces;
using System.Text;
using System.Text.Json;

namespace Orbito.API.Middleware;

/// <summary>
/// Middleware for handling idempotency in payment operations
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private readonly IdempotencySettings _settings;

    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger,
        IOptions<IdempotencySettings> settings)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task InvokeAsync(
        HttpContext context,
        IIdempotencyCacheService cacheService,
        ITenantContext tenantContext,
        IUserContextService userContextService)
    {
        // Only apply to POST requests to payment endpoints
        if (!ShouldProcessRequest(context))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = ExtractIdempotencyKey(context);
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            if (_settings.RequireIdempotencyKey)
            {
                _logger.LogWarning("Idempotency key is required but not provided for {Path}", context.Request.Path);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Idempotency key is required for this operation");
                return;
            }

            await _next(context);
            return;
        }

        // Validate idempotency key format
        if (!IsValidIdempotencyKey(idempotencyKey))
        {
            _logger.LogWarning("Invalid idempotency key format for {Path}: {Key}", context.Request.Path, idempotencyKey);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid idempotency key format");
            return;
        }

        var cacheKey = BuildCacheKey(idempotencyKey, context, tenantContext, userContextService);
        var lockAcquired = false;

        try
        {
            // Try to acquire distributed lock if enabled
            if (_settings.UseDistributedLock)
            {
                var lockTimeout = TimeSpan.FromSeconds(_settings.LockTimeoutSeconds);
                lockAcquired = await cacheService.TryAcquireLockAsync(cacheKey, lockTimeout);

                if (!lockAcquired)
                {
                    _logger.LogWarning("Failed to acquire lock for idempotency key {Key}", idempotencyKey);
                    context.Response.StatusCode = 409; // Conflict
                    await context.Response.WriteAsync("Request is being processed by another instance");
                    return;
                }

                // FIXED: Double-checked locking pattern - check cache INSIDE lock
                // This prevents race condition where two requests pass the first check
                var cachedResponse = await cacheService.TryGetCachedResponseAsync(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Returning cached response for idempotency key {Key} (duplicate request prevented)", idempotencyKey);
                    await WriteCachedResponse(context, cachedResponse);
                    return;
                }

                // Process the request and capture the response (protected by lock)
                await ProcessRequestWithResponseCapture(context, cacheKey, idempotencyKey, cacheService, tenantContext, userContextService);
            }
            else
            {
                // No lock: check cache before processing
                var cachedResponse = await cacheService.TryGetCachedResponseAsync(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Returning cached response for idempotency key {Key}", idempotencyKey);
                    await WriteCachedResponse(context, cachedResponse);
                    return;
                }

                // Process the request and capture the response
                await ProcessRequestWithResponseCapture(context, cacheKey, idempotencyKey, cacheService, tenantContext, userContextService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing idempotency for key {Key}", idempotencyKey);
            throw;
        }
        finally
        {
            // Release the lock if we acquired it
            if (lockAcquired && _settings.UseDistributedLock)
            {
                await cacheService.ReleaseLockAsync(cacheKey);
            }
        }
    }

    private bool ShouldProcessRequest(HttpContext context)
    {
        if (!_settings.Enabled)
            return false;

        if (context.Request.Method != "POST")
            return false;

        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        return path.StartsWith("/api/payments/") && 
               (path.Contains("/create") || path.Contains("/process") || path.Contains("/retry"));
    }

    private string? ExtractIdempotencyKey(HttpContext context)
    {
        // Try to get from header first
        var headerKey = context.Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerKey))
            return headerKey;

        // Try to get from query parameter as fallback
        var queryKey = context.Request.Query["idempotency_key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryKey))
            return queryKey;

        return null;
    }

    private bool IsValidIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (key.Length < _settings.MinKeyLength || key.Length > _settings.MaxKeyLength)
            return false;

        // Check if it's a valid GUID
        if (Guid.TryParse(key, out _))
            return true;

        // Check if it's a valid custom key (alphanumeric, hyphens, underscores)
        return key.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }

    private string BuildCacheKey(string idempotencyKey, HttpContext context, ITenantContext tenantContext, IUserContextService userContextService)
    {
        // FIXED: Require tenant and user context for security
        if (!tenantContext.HasTenant)
            throw new InvalidOperationException("Tenant context is required for idempotency operations");

        var userId = userContextService.GetCurrentUserId();
        if (userId == null)
            throw new InvalidOperationException("User context is required for idempotency operations");

        var tenantId = tenantContext.CurrentTenantId.Value.ToString();
        var clientId = userId.Value.ToString();

        // FIXED: Sanitize path to prevent cache key injection
        var normalizedPath = context.Request.Path.Value?.Replace("/", "_").Replace(":", "_") ?? "unknown";

        // FIXED: Sanitize idempotency key to prevent cache key injection
        var sanitizedKey = idempotencyKey.Replace(":", "_");

        return $"{_settings.CacheKeyPrefix}{tenantId}:{clientId}:{normalizedPath}:{sanitizedKey}";
    }

    private async Task ProcessRequestWithResponseCapture(HttpContext context, string cacheKey, string idempotencyKey, IIdempotencyCacheService cacheService, ITenantContext tenantContext, IUserContextService userContextService)
    {
        // Store the original response stream
        var originalResponseStream = context.Response.Body;

        try
        {
            // Create a memory stream to capture the response
            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Process the request
            await _next(context);

            // FIXED: Validate response size before caching (DOS protection)
            const int MaxResponseSizeBytes = 1048576; // 1MB
            if (responseStream.Length > MaxResponseSizeBytes)
            {
                _logger.LogWarning("Response too large to cache for idempotency key {Key}: {Size} bytes",
                    idempotencyKey, responseStream.Length);
                responseStream.Position = 0;
                await responseStream.CopyToAsync(originalResponseStream);
                return;
            }

            // Capture the response
            responseStream.Position = 0;
            var responseBody = await new StreamReader(responseStream).ReadToEndAsync();

            // FIXED: Only cache successful responses (2xx-3xx status codes)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 400)
            {
                // Create cache entry
                var cacheEntry = new IdempotencyCacheEntry
                {
                    StatusCode = context.Response.StatusCode,
                    Headers = context.Response.Headers.ToDictionary(
                        h => h.Key,
                        h => string.Join(", ", h.Value.ToArray())),
                    Body = responseBody,
                    ContentType = context.Response.ContentType ?? "application/json",
                    TenantId = tenantContext.HasTenant ? tenantContext.CurrentTenantId.Value : null,
                    ClientId = userContextService.GetCurrentUserId()
                };

                // Cache the response
                var ttl = TimeSpan.FromHours(_settings.CacheTtlHours);
                await cacheService.CacheResponseAsync(cacheKey, cacheEntry, ttl);

                _logger.LogInformation("Cached successful response for idempotency key {Key} with status {StatusCode}",
                    idempotencyKey, context.Response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Skipping cache for non-success response for idempotency key {Key} with status {StatusCode}",
                    idempotencyKey, context.Response.StatusCode);
            }

            // Write the response to the original stream
            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalResponseStream);
        }
        finally
        {
            // Restore the original response stream
            context.Response.Body = originalResponseStream;
        }
    }

    private async Task WriteCachedResponse(HttpContext context, IdempotencyCacheEntry cachedResponse)
    {
        context.Response.StatusCode = cachedResponse.StatusCode;
        context.Response.ContentType = cachedResponse.ContentType;

        // Set headers
        foreach (var header in cachedResponse.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        // Write the cached body
        await context.Response.WriteAsync(cachedResponse.Body);
    }
}

/// <summary>
/// Extension methods for registering the idempotency middleware
/// </summary>
public static class IdempotencyMiddlewareExtensions
{
    /// <summary>
    /// Adds idempotency middleware to the application pipeline
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
