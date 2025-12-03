using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
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
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to get active subscription plans without tenant context");
                return Result.Failure<ActiveSubscriptionPlansDto>(DomainErrors.Tenant.NoTenantContext);
            }

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

            var dto = new ActiveSubscriptionPlansDto
            {
                Plans = plans,
                TotalCount = plans.Count
            };

            _logger.LogInformation("Successfully retrieved {Count} active subscription plans (PublicOnly: {PublicOnly}, Limit: {Limit})", 
                plans.Count, request.PublicOnly, request.Limit);

            return Result.Success(dto);
        }
    }
}
