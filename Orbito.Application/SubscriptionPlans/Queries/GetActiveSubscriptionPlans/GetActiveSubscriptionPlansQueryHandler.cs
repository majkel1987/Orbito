using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;

public class GetActiveSubscriptionPlansQueryHandler : IRequestHandler<GetActiveSubscriptionPlansQuery, Result<ActiveSubscriptionPlansDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetActiveSubscriptionPlansQueryHandler> _logger;

    public GetActiveSubscriptionPlansQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<GetActiveSubscriptionPlansQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ActiveSubscriptionPlansDto>> Handle(GetActiveSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to get active subscription plans without valid tenant context");
            return Result.Failure<ActiveSubscriptionPlansDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetActivePlansAsync(
            request.PublicOnly,
            request.Limit,
            cancellationToken);

        var plans = SubscriptionPlanMapper.ToActiveDtos(subscriptionPlans);

        var dto = new ActiveSubscriptionPlansDto
        {
            Plans = plans,
            TotalCount = plans.Count
        };

        _logger.LogDebug("Successfully retrieved {Count} active subscription plans (PublicOnly: {PublicOnly}, Limit: {Limit})",
            plans.Count, request.PublicOnly, request.Limit);

        return Result.Success(dto);
    }
}
