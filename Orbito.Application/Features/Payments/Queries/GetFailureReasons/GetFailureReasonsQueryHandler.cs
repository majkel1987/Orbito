using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetFailureReasons;

namespace Orbito.Application.Features.Payments.Queries.GetFailureReasons;

/// <summary>
/// Handler for get failure reasons query
/// </summary>
public class GetFailureReasonsQueryHandler : IRequestHandler<GetFailureReasonsQuery, Dictionary<string, int>>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ILogger<GetFailureReasonsQueryHandler> _logger;

    public GetFailureReasonsQueryHandler(
        IPaymentMetricsService metricsService,
        ILogger<GetFailureReasonsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get failure reasons query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of failure reasons and their counts</returns>
    public async Task<Dictionary<string, int>> Handle(GetFailureReasonsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting failure reasons for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);

            var dateRange = request.GetDateRange();
            
            if (!dateRange.IsValid())
            {
                _logger.LogWarning("Invalid date range provided: {StartDate} to {EndDate}", 
                    request.StartDate, request.EndDate);
                return new Dictionary<string, int>();
            }

            var failureReasons = await _metricsService.GetFailureReasonsBreakdownAsync(
                dateRange, 
                request.ProviderId, 
                cancellationToken);

            _logger.LogInformation("Retrieved failure reasons: {FailureReasonsCount} different reasons found",
                failureReasons.Count);

            return failureReasons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failure reasons for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
                request.StartDate, request.EndDate, request.ProviderId);
            
            return new Dictionary<string, int>();
        }
    }
}
