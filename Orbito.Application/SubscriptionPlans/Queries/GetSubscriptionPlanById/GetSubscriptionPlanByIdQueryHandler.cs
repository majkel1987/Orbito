using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById
{
    public class GetSubscriptionPlanByIdQueryHandler : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public GetSubscriptionPlanByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<SubscriptionPlanDto?> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to get subscription plan");

            var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (subscriptionPlan == null)
                return null;

            var activeSubscriptionsCount = subscriptionPlan.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
            var totalSubscriptionsCount = subscriptionPlan.Subscriptions.Count;

            return new SubscriptionPlanDto
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
        }
    }
}
