using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentTrends;

/// <summary>
/// Handler for get payment trends query.
/// SECURITY: Validates tenant context and ensures users can only query their own provider's data.
/// </summary>
public class GetPaymentTrendsQueryHandler : IRequestHandler<GetPaymentTrendsQuery, Orbito.Domain.Common.Result<PaymentTrends>>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ITenantContext _tenantContext;
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<GetPaymentTrendsQueryHandler> _logger;

    public GetPaymentTrendsQueryHandler(
        IPaymentMetricsService metricsService,
        ITenantContext tenantContext,
        IProviderRepository providerRepository,
        ILogger<GetPaymentTrendsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _tenantContext = tenantContext;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get payment trends query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment trends for the specified period wrapped in Result</returns>
    public async Task<Orbito.Domain.Common.Result<PaymentTrends>> Handle(GetPaymentTrendsQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify tenant context exists
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempted to get payment trends without tenant context");
            return Orbito.Domain.Common.Result.Failure<PaymentTrends>(DomainErrors.Tenant.NoTenantContext);
        }

        var currentTenantId = _tenantContext.CurrentTenantId!;

        // SECURITY: If ProviderId is specified, verify it belongs to the current tenant
        Guid? authorizedProviderId = request.ProviderId;
        if (request.ProviderId.HasValue)
        {
            var provider = await _providerRepository.GetByIdAsync(request.ProviderId.Value, cancellationToken);
            if (provider == null)
            {
                _logger.LogWarning("Provider {ProviderId} not found", request.ProviderId);
                return Orbito.Domain.Common.Result.Failure<PaymentTrends>(DomainErrors.Provider.NotFound);
            }

            if (provider.TenantId != currentTenantId)
            {
                _logger.LogWarning("Cross-tenant access attempt: Tenant {CurrentTenant} tried to access provider {ProviderId}",
                    currentTenantId, request.ProviderId);
                return Orbito.Domain.Common.Result.Failure<PaymentTrends>(DomainErrors.Tenant.CrossTenantAccess);
            }
        }
        else
        {
            // If no ProviderId specified, use the current tenant's provider
            var provider = await _providerRepository.GetByTenantIdAsync(currentTenantId, cancellationToken);
            authorizedProviderId = provider?.Id;
        }

        _logger.LogDebug("Getting payment trends for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
            request.StartDate, request.EndDate, authorizedProviderId);

        // Note: DateRange validation is already done by FluentValidation pipeline
        var dateRange = request.GetDateRange();

        var trends = await _metricsService.GetPaymentTrendsAsync(
            dateRange,
            authorizedProviderId,
            cancellationToken);

        _logger.LogDebug("Retrieved payment trends: {DataPointsCount} data points, {OverallTrend} trend ({PercentageChange}% change)",
            trends.DataPoints.Count, trends.OverallTrend, trends.PercentageChange);

        return Orbito.Domain.Common.Result.Success(trends);
    }
}
