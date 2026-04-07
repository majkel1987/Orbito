using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

/// <summary>
/// Query to get comprehensive payment statistics for a specific period.
/// Returns Result&lt;PaymentStatistics&gt; for proper error handling.
/// </summary>
/// <param name="StartDate">Start date of the statistics period</param>
/// <param name="EndDate">End date of the statistics period</param>
/// <param name="ProviderId">Optional provider ID to filter statistics by. If not specified, uses current tenant's provider</param>
public record GetPaymentStatisticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? ProviderId = null) : IRequest<Orbito.Domain.Common.Result<PaymentStatistics>>
{
    /// <summary>
    /// Gets the date range for this query
    /// </summary>
    /// <returns>Date range</returns>
    public DateRange GetDateRange() => new(StartDate, EndDate);
}
