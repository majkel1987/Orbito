namespace Orbito.Application.Features.Analytics.Queries.GetDashboardStats;

/// <summary>
/// Dashboard statistics DTO with key performance indicators
/// </summary>
public record DashboardStatsDto
{
    /// <summary>
    /// Monthly Recurring Revenue - sum of all active subscription amounts for the current month
    /// </summary>
    public decimal Mrr { get; init; }

    /// <summary>
    /// Annual Recurring Revenue - MRR * 12
    /// </summary>
    public decimal Arr { get; init; }

    /// <summary>
    /// Total number of active clients
    /// </summary>
    public int TotalClients { get; init; }

    /// <summary>
    /// Number of active subscriptions
    /// </summary>
    public int ActiveSubscriptions { get; init; }

    /// <summary>
    /// Churn rate as percentage (0-100)
    /// </summary>
    public decimal ChurnRate { get; init; }

    /// <summary>
    /// Total revenue from all completed payments
    /// </summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Currency code (e.g., "PLN", "USD")
    /// </summary>
    public string Currency { get; init; } = "PLN";

    /// <summary>
    /// Number of new clients in the period
    /// </summary>
    public int NewClients { get; init; }

    /// <summary>
    /// Number of cancelled subscriptions in the period
    /// </summary>
    public int CancelledSubscriptions { get; init; }

    /// <summary>
    /// Date range this statistics cover
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Date range this statistics cover
    /// </summary>
    public DateTime EndDate { get; init; }
}
