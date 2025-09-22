using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
    public class GetActiveSubscriptionPlansQueryHandler : IRequestHandler<GetActiveSubscriptionPlansQuery, ActiveSubscriptionPlansDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public GetActiveSubscriptionPlansQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<ActiveSubscriptionPlansDto> Handle(GetActiveSubscriptionPlansQuery request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to get active subscription plans");

            var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetActivePlansAsync(
                request.PublicOnly,
                request.Limit,
                cancellationToken);

            var plans = subscriptionPlans.Select(plan => new ActiveSubscriptionPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                Amount = plan.Price.Amount,
                Currency = plan.Price.Currency,
                BillingPeriod = plan.BillingPeriod.ToString(),
                TrialPeriodDays = plan.TrialPeriodDays,
                FeaturesJson = plan.FeaturesJson,
                LimitationsJson = plan.LimitationsJson,
                SortOrder = plan.SortOrder,
                ActiveSubscriptionsCount = plan.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active)
            }).ToList();

            return new ActiveSubscriptionPlansDto
            {
                Plans = plans,
                TotalCount = plans.Count
            };
        }
    }
}
