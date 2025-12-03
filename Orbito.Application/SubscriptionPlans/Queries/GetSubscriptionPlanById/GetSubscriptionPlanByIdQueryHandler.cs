using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById
{
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
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to get subscription plan without tenant context");
                return Result.Failure<SubscriptionPlanDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (subscriptionPlan == null)
            {
                _logger.LogWarning("Subscription plan {PlanId} not found", request.Id);
                return Result.Failure<SubscriptionPlanDto>(DomainErrors.SubscriptionPlan.NotFound);
            }

            var activeSubscriptionsCount = subscriptionPlan.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
            var totalSubscriptionsCount = subscriptionPlan.Subscriptions.Count;

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
                ActiveSubscriptionsCount = activeSubscriptionsCount,
                TotalSubscriptionsCount = totalSubscriptionsCount
            };

            _logger.LogInformation("Successfully retrieved subscription plan {PlanId}", request.Id);

            return Result.Success(dto);
        }
    }
}
