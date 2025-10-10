using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Query to get revenue report for a specific provider and period
/// </summary>
public record GetRevenueReportQuery : IRequest<RevenueMetrics>
{
    /// <summary>
    /// Start date of the revenue period
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// End date of the revenue period
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Provider ID to get revenue for
    /// </summary>
    public Guid ProviderId { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetRevenueReportQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="providerId">Provider ID</param>
    public GetRevenueReportQuery(DateTime startDate, DateTime endDate, Guid providerId)
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
