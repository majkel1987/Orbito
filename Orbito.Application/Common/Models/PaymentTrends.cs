namespace Orbito.Application.Common.Models;

/// <summary>
/// Payment trends over time for a specific period
/// </summary>
public record PaymentTrends
{
    /// <summary>
    /// Trend data points grouped by time period
    /// </summary>
    public List<TrendDataPoint> DataPoints { get; init; } = new();

    /// <summary>
    /// Overall trend direction (increasing, decreasing, stable)
    /// </summary>
    public TrendDirection OverallTrend { get; init; }

    /// <summary>
    /// Percentage change from first to last data point
    /// </summary>
    public decimal PercentageChange { get; init; }

    /// <summary>
    /// Date range this trends data covers
    /// </summary>
    public DateRange Period { get; init; } = new();

    /// <summary>
    /// Provider ID these trends belong to (null for all providers)
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// When these trends were calculated
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Single data point in a trend
/// </summary>
public record TrendDataPoint
{
    /// <summary>
    /// Date/time this data point represents
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Number of payments in this period
    /// </summary>
    public int PaymentCount { get; init; }

    /// <summary>
    /// Number of successful payments in this period
    /// </summary>
    public int SuccessfulPayments { get; init; }

    /// <summary>
    /// Number of failed payments in this period
    /// </summary>
    public int FailedPayments { get; init; }

    /// <summary>
    /// Total revenue in this period
    /// </summary>
    public decimal Revenue { get; init; }

    /// <summary>
    /// Currency of the revenue
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Success rate for this period (0-100)
    /// </summary>
    public decimal SuccessRate { get; init; }
}

/// <summary>
/// Direction of a trend
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// Trend is increasing
    /// </summary>
    Increasing,

    /// <summary>
    /// Trend is decreasing
    /// </summary>
    Decreasing,

    /// <summary>
    /// Trend is stable (no significant change)
    /// </summary>
    Stable
}
