using Orbito.Application.Common.Models;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service interface for advanced payment metrics and statistics
/// </summary>
public interface IPaymentMetricsService
{
    /// <summary>
    /// Gets payment success rate for a specific date range and optional provider
    /// </summary>
    /// <param name="range">Date range for the metrics</param>
    /// <param name="providerId">Optional provider ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment success rate percentage</returns>
    Task<decimal> GetPaymentSuccessRateAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets average payment processing time for a specific date range
    /// </summary>
    /// <param name="range">Date range for the metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Average processing time in seconds</returns>
    Task<decimal> GetAverageProcessingTimeAsync(DateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets breakdown of failure reasons for a specific date range and optional provider
    /// </summary>
    /// <param name="range">Date range for the metrics</param>
    /// <param name="providerId">Optional provider ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of failure reasons and their counts</returns>
    Task<Dictionary<string, int>> GetFailureReasonsBreakdownAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revenue metrics for a specific provider and date range
    /// </summary>
    /// <param name="range">Date range for the metrics</param>
    /// <param name="providerId">Provider ID to get revenue for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revenue metrics including total, growth, and currency breakdown</returns>
    Task<RevenueMetrics> GetRevenueMetricsAsync(DateRange range, Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comprehensive payment statistics for a specific date range
    /// </summary>
    /// <param name="range">Date range for the statistics</param>
    /// <param name="providerId">Optional provider ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive payment statistics</returns>
    Task<PaymentStatistics> GetPaymentStatisticsAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment trends over time for a specific date range
    /// </summary>
    /// <param name="range">Date range for the trends</param>
    /// <param name="providerId">Optional provider ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment trends grouped by time period</returns>
    Task<PaymentTrends> GetPaymentTrendsAsync(DateRange range, Guid? providerId = null, CancellationToken cancellationToken = default);
}
