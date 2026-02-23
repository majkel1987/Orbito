using MediatR;

namespace Orbito.Application.Features.Analytics.Queries.GetRevenueHistory;

/// <summary>
/// Query to get revenue history for charts
/// </summary>
public record GetRevenueHistoryQuery : IRequest<GetRevenueHistoryResponse>
{
    /// <summary>
    /// Start date of the period
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the period
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetRevenueHistoryQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    public GetRevenueHistoryQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
