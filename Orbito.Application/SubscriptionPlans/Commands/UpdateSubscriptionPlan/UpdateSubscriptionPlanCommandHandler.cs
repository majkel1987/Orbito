using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, Result<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateSubscriptionPlanCommandHandler> _logger;

    public UpdateSubscriptionPlanCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<UpdateSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.HasTenant || _tenantContext.CurrentTenantId == null)
        {
            _logger.LogWarning("Attempted to update subscription plan without valid tenant context");
            return Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found", request.Id);
            return Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound);
        }

        // Update basic properties
        var basicInfoResult = subscriptionPlan.UpdateBasicInfo(
            request.Name,
            request.Description,
            request.TrialDays,
            request.TrialPeriodDays,
            request.SortOrder);
        if (basicInfoResult.IsFailure)
            return Result.Failure<SubscriptionPlanDto>(basicInfoResult.Error);

        // Update price
        subscriptionPlan.UpdatePrice(Money.Create(request.Amount, request.Currency));

        // Update billing period
        var billingResult = subscriptionPlan.UpdateBillingPeriod(BillingPeriod.Create(1, request.BillingPeriodType));
        if (billingResult.IsFailure)
            return Result.Failure<SubscriptionPlanDto>(billingResult.Error);

        // Update features and limitations
        subscriptionPlan.UpdateFeatures(request.FeaturesJson);
        subscriptionPlan.UpdateLimitations(request.LimitationsJson);

        // Update status and visibility
        if (request.IsActive)
            subscriptionPlan.Activate();
        else
            subscriptionPlan.Deactivate();

        subscriptionPlan.UpdateVisibility(request.IsPublic);

        await _unitOfWork.SubscriptionPlans.UpdateAsync(subscriptionPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated subscription plan {PlanId} with name {Name}", subscriptionPlan.Id, subscriptionPlan.Name);

        return Result.Success(SubscriptionPlanMapper.ToDtoWithoutCounts(subscriptionPlan));
    }
}
