using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

/// <summary>
/// Handler for get payment statistics query.
/// SECURITY: Validates tenant context and ensures users can only query their own provider's data.
/// </summary>
public class GetPaymentStatisticsQueryHandler : IRequestHandler<GetPaymentStatisticsQuery, Orbito.Domain.Common.Result<PaymentStatistics>>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ITenantContext _tenantContext;
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<GetPaymentStatisticsQueryHandler> _logger;

    public GetPaymentStatisticsQueryHandler(
        IPaymentMetricsService metricsService,
        ITenantContext tenantContext,
        IProviderRepository providerRepository,
        ILogger<GetPaymentStatisticsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _tenantContext = tenantContext;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get payment statistics query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment statistics for the specified period wrapped in Result</returns>
    public async Task<Orbito.Domain.Common.Result<PaymentStatistics>> Handle(GetPaymentStatisticsQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify tenant context exists
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempted to get payment statistics without tenant context");
            return Orbito.Domain.Common.Result.Failure<PaymentStatistics>(DomainErrors.Tenant.NoTenantContext);
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
                return Orbito.Domain.Common.Result.Failure<PaymentStatistics>(DomainErrors.Provider.NotFound);
            }

            if (provider.TenantId != currentTenantId)
            {
                _logger.LogWarning("Cross-tenant access attempt: Tenant {CurrentTenant} tried to access provider {ProviderId}",
                    currentTenantId, request.ProviderId);
                return Orbito.Domain.Common.Result.Failure<PaymentStatistics>(DomainErrors.Tenant.CrossTenantAccess);
            }
        }
        else
        {
            // If no ProviderId specified, use the current tenant's provider
            var provider = await _providerRepository.GetByTenantIdAsync(currentTenantId, cancellationToken);
            authorizedProviderId = provider?.Id;
        }

        _logger.LogDebug("Getting payment statistics for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
            request.StartDate, request.EndDate, authorizedProviderId);

        // Note: DateRange validation is already done by FluentValidation pipeline
        var dateRange = request.GetDateRange();

        var statistics = await _metricsService.GetPaymentStatisticsAsync(
            dateRange,
            authorizedProviderId,
            cancellationToken);

        _logger.LogDebug("Retrieved payment statistics: {TotalPayments} total payments, {SuccessRate}% success rate",
            statistics.TotalPayments, statistics.SuccessRate);

        return Orbito.Domain.Common.Result.Success(statistics);
    }
}
