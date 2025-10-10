using MediatR;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

/// <summary>
/// Query to get comprehensive payment statistics for a specific period
/// </summary>
public record GetPaymentStatisticsQuery : IRequest<PaymentStatistics>
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
    /// Optional provider ID to filter statistics by
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetPaymentStatisticsQuery()
    {
    }

    /// <summary>
    /// Constructor with parameters
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="providerId">Optional provider ID</param>
    public GetPaymentStatisticsQuery(DateTime startDate, DateTime endDate, Guid? providerId = null)
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
