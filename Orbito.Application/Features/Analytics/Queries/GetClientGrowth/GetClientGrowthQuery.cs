using MediatR;

namespace Orbito.Application.Features.Analytics.Queries.GetClientGrowth;

/// <summary>
/// Query to get client growth history for charts
/// </summary>
public record GetClientGrowthQuery : IRequest<GetClientGrowthResponse>
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
    public GetClientGrowthQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    public GetClientGrowthQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}
