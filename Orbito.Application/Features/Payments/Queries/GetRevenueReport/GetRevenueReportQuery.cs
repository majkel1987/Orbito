using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Query to get revenue report for a specific provider and period.
/// Returns Result&lt;RevenueMetrics&gt; for proper error handling.
/// </summary>
/// <param name="StartDate">Start date of the revenue period</param>
/// <param name="EndDate">End date of the revenue period</param>
/// <param name="ProviderId">Provider ID to get revenue for (required, must belong to current tenant)</param>
public record GetRevenueReportQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid ProviderId) : IRequest<Orbito.Domain.Common.Result<RevenueMetrics>>
{
    /// <summary>
    /// Gets the date range for this query
    /// </summary>
    /// <returns>Date range</returns>
    public DateRange GetDateRange() => new(StartDate, EndDate);
}
