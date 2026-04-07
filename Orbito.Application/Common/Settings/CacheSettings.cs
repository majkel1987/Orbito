namespace Orbito.Application.Common.Settings;

/// <summary>
/// Configuration settings for caching functionality
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Default absolute expiration time in hours for cache entries
    /// </summary>
    public int DefaultAbsoluteExpirationHours { get; set; } = 1;

    /// <summary>
    /// Whether to enable caching globally
    /// </summary>
    public bool EnableCaching { get; set; } = true;
}
