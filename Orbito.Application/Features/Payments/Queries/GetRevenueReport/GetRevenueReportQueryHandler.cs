using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Handler for get revenue report query.
/// SECURITY: Validates tenant context and ensures users can only query their own provider's data.
/// </summary>
public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, Orbito.Domain.Common.Result<RevenueMetrics>>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ITenantContext _tenantContext;
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<GetRevenueReportQueryHandler> _logger;

    public GetRevenueReportQueryHandler(
        IPaymentMetricsService metricsService,
        ITenantContext tenantContext,
        IProviderRepository providerRepository,
        ILogger<GetRevenueReportQueryHandler> logger)
    {
        _metricsService = metricsService;
        _tenantContext = tenantContext;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get revenue report query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revenue metrics for the specified provider and period wrapped in Result</returns>
    public async Task<Orbito.Domain.Common.Result<RevenueMetrics>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify tenant context exists
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempted to get revenue report without tenant context");
            return Orbito.Domain.Common.Result.Failure<RevenueMetrics>(DomainErrors.Tenant.NoTenantContext);
        }

        var currentTenantId = _tenantContext.CurrentTenantId!;

        // SECURITY: Verify the requested ProviderId belongs to the current tenant
        var provider = await _providerRepository.GetByIdAsync(request.ProviderId, cancellationToken);
        if (provider == null)
        {
            _logger.LogWarning("Provider {ProviderId} not found", request.ProviderId);
            return Orbito.Domain.Common.Result.Failure<RevenueMetrics>(DomainErrors.Provider.NotFound);
        }

        if (provider.TenantId != currentTenantId)
        {
            _logger.LogWarning("Cross-tenant access attempt: Tenant {CurrentTenant} tried to access revenue for provider {ProviderId}",
                currentTenantId, request.ProviderId);
            return Orbito.Domain.Common.Result.Failure<RevenueMetrics>(DomainErrors.Tenant.CrossTenantAccess);
        }

        _logger.LogDebug("Getting revenue report for provider {ProviderId} in period {StartDate} to {EndDate}",
            request.ProviderId, request.StartDate, request.EndDate);

        // Note: DateRange validation is already done by FluentValidation pipeline
        var dateRange = request.GetDateRange();

        var revenueMetrics = await _metricsService.GetRevenueMetricsAsync(
            dateRange,
            request.ProviderId,
            cancellationToken);

        _logger.LogDebug("Retrieved revenue report for provider {ProviderId}: {TotalRevenue} {Currency} from {PaymentCount} payments",
            request.ProviderId, revenueMetrics.TotalRevenue, revenueMetrics.Currency, revenueMetrics.SuccessfulPaymentsCount);

        return Orbito.Domain.Common.Result.Success(revenueMetrics);
    }
}
