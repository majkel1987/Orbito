namespace Orbito.Application.Features.Analytics.Queries.GetClientGrowth;

/// <summary>
/// Single client growth data point for charts
/// </summary>
public record ClientGrowthItemDto
{
    /// <summary>
    /// Date of the data point
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Total number of clients as of this date
    /// </summary>
    public int TotalClients { get; init; }

    /// <summary>
    /// Number of new clients on this date
    /// </summary>
    public int NewClients { get; init; }
}

/// <summary>
/// Response containing client growth data
/// </summary>
public record GetClientGrowthResponse
{
    /// <summary>
    /// List of client growth data points
    /// </summary>
    public List<ClientGrowthItemDto> Items { get; init; } = new();

    /// <summary>
    /// Total clients at end of period
    /// </summary>
    public int TotalClients { get; init; }

    /// <summary>
    /// Total new clients in period
    /// </summary>
    public int NewClientsInPeriod { get; init; }

    /// <summary>
    /// Start date of the period
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the period
    /// </summary>
    public DateTime EndDate { get; init; }
}
