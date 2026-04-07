using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;

public class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, Result<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetSubscriptionPlanByIdQueryHandler> _logger;

    public GetSubscriptionPlanByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetSubscriptionPlanByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to get subscription plan without valid tenant context");
            return Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdWithSubscriptionsAsync(request.Id, cancellationToken);
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found", request.Id);
            return Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound);
        }

        _logger.LogDebug("Successfully retrieved subscription plan {PlanId}", request.Id);

        return Result.Success(SubscriptionPlanMapper.ToDto(subscriptionPlan));
    }
}
