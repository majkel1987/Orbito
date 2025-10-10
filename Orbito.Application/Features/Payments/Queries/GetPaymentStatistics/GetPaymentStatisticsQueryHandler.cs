using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

/// <summary>
/// Handler for get payment statistics query
/// </summary>
public class GetPaymentStatisticsQueryHandler : IRequestHandler<GetPaymentStatisticsQuery, PaymentStatistics>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ILogger<GetPaymentStatisticsQueryHandler> _logger;

    public GetPaymentStatisticsQueryHandler(
        IPaymentMetricsService metricsService,
        ILogger<GetPaymentStatisticsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get payment statistics query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment statistics for the specified period</returns>
    public async Task<PaymentStatistics> Handle(GetPaymentStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment statistics for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);

            var dateRange = request.GetDateRange();
            
            if (!dateRange.IsValid())
            {
                _logger.LogWarning("Invalid date range provided: {StartDate} to {EndDate}", 
                    request.StartDate, request.EndDate);
                return new PaymentStatistics 
                { 
                    Period = dateRange, 
                    ProviderId = request.ProviderId 
                };
            }

            var statistics = await _metricsService.GetPaymentStatisticsAsync(
                dateRange, 
                request.ProviderId, 
                cancellationToken);

            _logger.LogInformation("Retrieved payment statistics: {TotalPayments} total payments, {SuccessRate}% success rate",
                statistics.TotalPayments, statistics.SuccessRate);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);
            
            return new PaymentStatistics 
            { 
                Period = request.GetDateRange(), 
                ProviderId = request.ProviderId 
            };
        }
    }
}
