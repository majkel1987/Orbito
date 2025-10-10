namespace Orbito.Application.Common.Constants;

/// <summary>
/// Constants for cache TTL values
/// </summary>
public static class CacheConstants
{
    /// <summary>
    /// Short cache TTL for frequently changing data (5 minutes)
    /// </summary>
    public static readonly TimeSpan ShortCacheTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Medium cache TTL for moderately changing data (15 minutes)
    /// </summary>
    public static readonly TimeSpan MediumCacheTtl = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Long cache TTL for rarely changing data (1 hour)
    /// </summary>
    public static readonly TimeSpan LongCacheTtl = TimeSpan.FromHours(1);

    /// <summary>
    /// Very long cache TTL for static data (24 hours)
    /// </summary>
    public static readonly TimeSpan VeryLongCacheTtl = TimeSpan.FromHours(24);

    /// <summary>
    /// Cache key prefixes
    /// </summary>
    public static class KeyPrefixes
    {
        public const string PaymentSuccessRate = "success_rate";
        public const string PaymentStatistics = "payment_stats";
        public const string RevenueMetrics = "revenue_metrics";
        public const string PaymentTrends = "payment_trends";
        public const string FailureReasons = "failure_reasons";
        public const string ProcessingTime = "processing_time";
    }
}
