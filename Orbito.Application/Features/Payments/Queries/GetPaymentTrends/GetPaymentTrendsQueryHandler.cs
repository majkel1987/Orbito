using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetPaymentTrends;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentTrends;

/// <summary>
/// Handler for get payment trends query
/// </summary>
public class GetPaymentTrendsQueryHandler : IRequestHandler<GetPaymentTrendsQuery, PaymentTrends>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ILogger<GetPaymentTrendsQueryHandler> _logger;

    public GetPaymentTrendsQueryHandler(
        IPaymentMetricsService metricsService,
        ILogger<GetPaymentTrendsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get payment trends query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment trends for the specified period</returns>
    public async Task<PaymentTrends> Handle(GetPaymentTrendsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment trends for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);

            var dateRange = request.GetDateRange();
            
            if (!dateRange.IsValid())
            {
                _logger.LogWarning("Invalid date range provided: {StartDate} to {EndDate}", 
                    request.StartDate, request.EndDate);
                return new PaymentTrends 
                { 
                    Period = dateRange, 
                    ProviderId = request.ProviderId 
                };
            }

            var trends = await _metricsService.GetPaymentTrendsAsync(
                dateRange, 
                request.ProviderId, 
                cancellationToken);

            _logger.LogInformation("Retrieved payment trends: {DataPointsCount} data points, {OverallTrend} trend ({PercentageChange}% change)",
                trends.DataPoints.Count, trends.OverallTrend, trends.PercentageChange);

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment trends for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);
            
            return new PaymentTrends 
            { 
                Period = request.GetDateRange(), 
                ProviderId = request.ProviderId 
            };
        }
    }
}
