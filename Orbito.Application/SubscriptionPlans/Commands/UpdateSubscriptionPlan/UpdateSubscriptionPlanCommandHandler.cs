using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan
{
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
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to update subscription plan without tenant context");
                return Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (subscriptionPlan == null)
            {
                _logger.LogWarning("Subscription plan {PlanId} not found", request.Id);
                return Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound);
            }

            // Update basic properties
            subscriptionPlan.Name = request.Name;
            subscriptionPlan.Description = request.Description;
            subscriptionPlan.TrialDays = request.TrialDays;
            subscriptionPlan.TrialPeriodDays = request.TrialPeriodDays;
            subscriptionPlan.SortOrder = request.SortOrder;

            // Update price
            subscriptionPlan.UpdatePrice(Money.Create(request.Amount, request.Currency));

            // Update billing period
            subscriptionPlan.BillingPeriod = BillingPeriod.Create(1, request.BillingPeriodType);

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

            var dto = new SubscriptionPlanDto
            {
                Id = subscriptionPlan.Id,
                Name = subscriptionPlan.Name,
                Description = subscriptionPlan.Description,
                Amount = subscriptionPlan.Price.Amount,
                Currency = subscriptionPlan.Price.Currency,
                BillingPeriod = subscriptionPlan.BillingPeriod.ToString(),
                TrialDays = subscriptionPlan.TrialDays,
                TrialPeriodDays = subscriptionPlan.TrialPeriodDays,
                FeaturesJson = subscriptionPlan.FeaturesJson,
                LimitationsJson = subscriptionPlan.LimitationsJson,
                IsActive = subscriptionPlan.IsActive,
                IsPublic = subscriptionPlan.IsPublic,
                SortOrder = subscriptionPlan.SortOrder,
                CreatedAt = subscriptionPlan.CreatedAt,
                UpdatedAt = subscriptionPlan.UpdatedAt,
                ActiveSubscriptionsCount = 0,
                TotalSubscriptionsCount = 0
            };

            _logger.LogInformation("Updated subscription plan {PlanId} with name {Name}", subscriptionPlan.Id, subscriptionPlan.Name);

            return Result.Success(dto);
        }
    }
}
