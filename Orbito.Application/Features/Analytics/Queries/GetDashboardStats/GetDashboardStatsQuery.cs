using MediatR;

namespace Orbito.Application.Features.Analytics.Queries.GetDashboardStats;

/// <summary>
/// Query to get dashboard statistics for a specific date range
/// </summary>
public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
    /// <summary>
    /// Start date of the statistics period
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the statistics period
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetDashboardStatsQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    public GetDashboardStatsQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
