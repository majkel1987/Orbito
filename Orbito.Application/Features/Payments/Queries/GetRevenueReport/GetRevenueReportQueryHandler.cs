using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetRevenueReport;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Handler for get revenue report query
/// </summary>
public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueMetrics>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ILogger<GetRevenueReportQueryHandler> _logger;

    public GetRevenueReportQueryHandler(
        IPaymentMetricsService metricsService,
        ILogger<GetRevenueReportQueryHandler> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get revenue report query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revenue metrics for the specified provider and period</returns>
    public async Task<RevenueMetrics> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting revenue report for provider {ProviderId} in period {StartDate} to {EndDate}",
                request.ProviderId, request.StartDate, request.EndDate);

            var dateRange = request.GetDateRange();
            
            if (!dateRange.IsValid())
            {
                _logger.LogWarning("Invalid date range provided: {StartDate} to {EndDate}", 
                    request.StartDate, request.EndDate);
                return new RevenueMetrics 
                { 
                    Period = dateRange, 
                    ProviderId = request.ProviderId 
                };
            }

            var revenueMetrics = await _metricsService.GetRevenueMetricsAsync(
                dateRange, 
                request.ProviderId, 
                cancellationToken);

            _logger.LogInformation("Retrieved revenue report for provider {ProviderId}: {TotalRevenue} {Currency} from {PaymentCount} payments",
                request.ProviderId, revenueMetrics.TotalRevenue, revenueMetrics.Currency, revenueMetrics.SuccessfulPaymentsCount);

            return revenueMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue report for provider {ProviderId} in period {StartDate} to {EndDate}",
                request.ProviderId, request.StartDate, request.EndDate);
            
            return new RevenueMetrics 
            { 
                Period = request.GetDateRange(), 
                ProviderId = request.ProviderId 
            };
        }
    }
}
