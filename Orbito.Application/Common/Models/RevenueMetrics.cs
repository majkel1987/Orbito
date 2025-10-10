namespace Orbito.Application.Common.Models;

/// <summary>
/// Revenue metrics for a specific period and provider
/// </summary>
public record RevenueMetrics
{
    /// <summary>
    /// Total revenue for the period
    /// </summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Currency of the revenue
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Revenue growth compared to previous period (percentage)
    /// </summary>
    public decimal GrowthPercentage { get; init; }

    /// <summary>
    /// Revenue breakdown by currency
    /// </summary>
    public Dictionary<string, decimal> RevenueByCurrency { get; init; } = new();

    /// <summary>
    /// Average revenue per payment
    /// </summary>
    public decimal AverageRevenuePerPayment { get; init; }

    /// <summary>
    /// Number of successful payments that generated revenue
    /// </summary>
    public int SuccessfulPaymentsCount { get; init; }

    /// <summary>
    /// Revenue by payment method
    /// </summary>
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; init; } = new();

    /// <summary>
    /// Monthly recurring revenue (MRR) if applicable
    /// </summary>
    public decimal MonthlyRecurringRevenue { get; init; }

    /// <summary>
    /// Date range this metrics covers
    /// </summary>
    public DateRange Period { get; init; } = new();

    /// <summary>
    /// Provider ID these metrics belong to
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// When these metrics were calculated
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}
