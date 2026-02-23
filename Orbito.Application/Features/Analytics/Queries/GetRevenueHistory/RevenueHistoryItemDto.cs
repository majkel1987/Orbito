namespace Orbito.Application.Features.Analytics.Queries.GetRevenueHistory;

/// <summary>
/// Single revenue data point for charts
/// </summary>
public record RevenueHistoryItemDto
{
    /// <summary>
    /// Date of the revenue data point
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Revenue amount for this date
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; init; } = "PLN";
}

/// <summary>
/// Response containing revenue history data
/// </summary>
public record GetRevenueHistoryResponse
{
    /// <summary>
    /// List of revenue data points
    /// </summary>
    public List<RevenueHistoryItemDto> Items { get; init; } = new();

    /// <summary>
    /// Total revenue for the period
    /// </summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Start date of the period
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the period
    /// </summary>
    public DateTime EndDate { get; init; }
}
