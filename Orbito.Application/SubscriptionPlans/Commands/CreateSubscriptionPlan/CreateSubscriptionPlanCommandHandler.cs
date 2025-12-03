using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, Result<SubscriptionPlanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<CreateSubscriptionPlanCommandHandler> _logger;

        public CreateSubscriptionPlanCommandHandler(
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            ILogger<CreateSubscriptionPlanCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionPlanDto>> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to create subscription plan without tenant context");
                return Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantContext.CurrentTenantId!,
                request.Name,
                request.Amount,
                request.Currency,
                request.BillingPeriodType,
                request.Description,
                request.TrialDays,
                request.TrialPeriodDays,
                request.FeaturesJson,
                request.LimitationsJson,
                request.SortOrder);

            subscriptionPlan.UpdateVisibility(request.IsPublic);

            await _unitOfWork.SubscriptionPlans.AddAsync(subscriptionPlan, cancellationToken);
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

            _logger.LogInformation("Created subscription plan {PlanId} with name {Name}", subscriptionPlan.Id, subscriptionPlan.Name);

            return Result.Success(dto);
        }
    }
}
