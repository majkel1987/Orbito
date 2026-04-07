using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetFailureReasons;

/// <summary>
/// Query to get failure reasons breakdown for a specific period.
/// Returns Result&lt;FailureReasonsResponse&gt; for proper error handling.
/// </summary>
/// <param name="StartDate">Start date of the period</param>
/// <param name="EndDate">End date of the period</param>
/// <param name="ProviderId">Optional provider ID to filter failure reasons by. If not specified, uses current tenant's provider</param>
/// <param name="TopN">Optional limit for top N failure reasons (prevents unbounded result sets)</param>
public record GetFailureReasonsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? ProviderId = null,
    int? TopN = null) : IRequest<Orbito.Domain.Common.Result<FailureReasonsResponse>>
{
    /// <summary>
    /// Gets the date range for this query
    /// </summary>
    /// <returns>Date range</returns>
    public DateRange GetDateRange() => new(StartDate, EndDate);
}

/// <summary>
/// Response containing failure reasons breakdown with metadata
/// </summary>
public record FailureReasonsResponse
{
    /// <summary>
    /// Dictionary of failure reasons and their counts
    /// </summary>
    public required Dictionary<string, int> FailureReasons { get; init; }

    /// <summary>
    /// Total number of distinct failure reasons (before TopN limit)
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The date range for this query
    /// </summary>
    public DateRange Period { get; init; }

    /// <summary>
    /// The provider ID used for this query (may differ from requested if defaulted)
    /// </summary>
    public Guid? ProviderId { get; init; }
}
