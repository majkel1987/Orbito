using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetFailureReasons;

/// <summary>
/// Query to get failure reasons breakdown for a specific period
/// </summary>
public record GetFailureReasonsQuery : IRequest<Dictionary<string, int>>
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
    /// Optional provider ID to filter failure reasons by
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetFailureReasonsQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="providerId">Optional provider ID</param>
    public GetFailureReasonsQuery(DateTime startDate, DateTime endDate, Guid? providerId = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        ProviderId = providerId;
    }

    /// <summary>
    /// Gets the date range for this query
    /// </summary>
    /// <returns>Date range</returns>
    public DateRange GetDateRange() => new(StartDate, EndDate);
}
