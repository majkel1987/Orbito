using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Features.ProviderSubscriptions.Queries.GetMyProviderSubscription;

/// <summary>
/// Handler for getting the current provider's platform subscription.
/// Used for displaying trial banner and subscription status in the dashboard.
/// </summary>
public class GetMyProviderSubscriptionQueryHandler
    : IRequestHandler<GetMyProviderSubscriptionQuery, Result<ProviderSubscriptionDto>>
{
    private readonly IProviderSubscriptionRepository _subscriptionRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetMyProviderSubscriptionQueryHandler> _logger;

    public GetMyProviderSubscriptionQueryHandler(
        IProviderSubscriptionRepository subscriptionRepository,
        ITenantContext tenantContext,
        ILogger<GetMyProviderSubscriptionQueryHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ProviderSubscriptionDto>> Handle(
        GetMyProviderSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
        {
            _logger.LogWarning("Cannot get provider subscription: no tenant context");
            return Result.Failure<ProviderSubscriptionDto>(DomainErrors.Provider.NotFound);
        }

        _logger.LogDebug(
            "Getting provider subscription for tenant {TenantId}",
            tenantId.Value);

        var subscription = await _subscriptionRepository.GetByTenantIdAsync(
            tenantId.Value,
            cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning(
                "Provider subscription not found for tenant {TenantId}",
                tenantId.Value);
            return Result.Failure<ProviderSubscriptionDto>(DomainErrors.ProviderSubscription.NotFound);
        }

        var dto = new ProviderSubscriptionDto
        {
            Id = subscription.Id,
            PlatformPlanId = subscription.PlatformPlanId,
            Status = subscription.Status.ToString(),
            PlanName = subscription.PlatformPlan?.Name ?? "Unknown",
            PlanPrice = subscription.PlatformPlan?.Price?.Amount ?? 0,
            PlanCurrency = subscription.PlatformPlan?.Price?.Currency ?? "PLN",
            DaysRemaining = subscription.DaysRemaining,
            TrialEndDate = subscription.TrialEndDate,
            PaidUntil = subscription.PaidUntil,
            IsTrialActive = subscription.IsTrialActive,
            IsExpired = subscription.IsExpired
        };

        _logger.LogDebug(
            "Retrieved provider subscription {SubscriptionId} with status {Status}, {DaysRemaining} days remaining",
            subscription.Id,
            dto.Status,
            dto.DaysRemaining);

        return Result.Success(dto);
    }
}
