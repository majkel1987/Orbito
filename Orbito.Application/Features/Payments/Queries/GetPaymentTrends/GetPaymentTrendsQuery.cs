using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentTrends;

/// <summary>
/// Query to get payment trends over time for a specific period.
/// Returns Result&lt;PaymentTrends&gt; for proper error handling.
/// </summary>
/// <param name="StartDate">Start date of the trends period</param>
/// <param name="EndDate">End date of the trends period</param>
/// <param name="ProviderId">Optional provider ID to filter trends by. If not specified, uses current tenant's provider</param>
public record GetPaymentTrendsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Guid? ProviderId = null) : IRequest<Orbito.Domain.Common.Result<PaymentTrends>>
{
    /// <summary>
    /// Gets the date range for this query
    /// </summary>
    /// <returns>Date range</returns>
    public DateRange GetDateRange() => new(StartDate, EndDate);
}
