using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.Payments.Queries.GetFailureReasons;

/// <summary>
/// Handler for get failure reasons query.
/// SECURITY: Validates tenant context and ensures users can only query their own provider's data.
/// </summary>
public class GetFailureReasonsQueryHandler : IRequestHandler<GetFailureReasonsQuery, Orbito.Domain.Common.Result<FailureReasonsResponse>>
{
    private readonly IPaymentMetricsService _metricsService;
    private readonly ITenantContext _tenantContext;
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<GetFailureReasonsQueryHandler> _logger;

    public GetFailureReasonsQueryHandler(
        IPaymentMetricsService metricsService,
        ITenantContext tenantContext,
        IProviderRepository providerRepository,
        ILogger<GetFailureReasonsQueryHandler> logger)
    {
        _metricsService = metricsService;
        _tenantContext = tenantContext;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the get failure reasons query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of failure reasons and their counts wrapped in Result</returns>
    public async Task<Orbito.Domain.Common.Result<FailureReasonsResponse>> Handle(GetFailureReasonsQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify tenant context exists
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempted to get failure reasons without tenant context");
            return Orbito.Domain.Common.Result.Failure<FailureReasonsResponse>(DomainErrors.Tenant.NoTenantContext);
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
                return Orbito.Domain.Common.Result.Failure<FailureReasonsResponse>(DomainErrors.Provider.NotFound);
            }

            if (provider.TenantId != currentTenantId)
            {
                _logger.LogWarning("Cross-tenant access attempt: Tenant {CurrentTenant} tried to access failure reasons for provider {ProviderId}",
                    currentTenantId, request.ProviderId);
                return Orbito.Domain.Common.Result.Failure<FailureReasonsResponse>(DomainErrors.Tenant.CrossTenantAccess);
            }
        }
        else
        {
            // If no ProviderId specified, use the current tenant's provider
            var provider = await _providerRepository.GetByTenantIdAsync(currentTenantId, cancellationToken);
            authorizedProviderId = provider?.Id;
        }

        _logger.LogDebug("Getting failure reasons for period {StartDate} to {EndDate}, ProviderId: {ProviderId}",
            request.StartDate, request.EndDate, authorizedProviderId);

        // Note: DateRange validation is already done by FluentValidation pipeline
        var dateRange = request.GetDateRange();

        var failureReasons = await _metricsService.GetFailureReasonsBreakdownAsync(
            dateRange,
            authorizedProviderId,
            cancellationToken);

        // Apply TopN limit if specified
        var limitedReasons = request.TopN.HasValue
            ? failureReasons
                .OrderByDescending(x => x.Value)
                .Take(request.TopN.Value)
                .ToDictionary(x => x.Key, x => x.Value)
            : failureReasons;

        _logger.LogDebug("Retrieved failure reasons: {FailureReasonsCount} different reasons found (showing top {TopN})",
            failureReasons.Count, request.TopN ?? failureReasons.Count);

        return Orbito.Domain.Common.Result.Success(new FailureReasonsResponse
        {
            FailureReasons = limitedReasons,
            TotalCount = failureReasons.Count,
            Period = dateRange,
            ProviderId = authorizedProviderId
        });
    }
}
