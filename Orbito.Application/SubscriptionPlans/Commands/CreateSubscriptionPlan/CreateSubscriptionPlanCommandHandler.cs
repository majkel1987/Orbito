using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, CreateSubscriptionPlanResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public CreateSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<CreateSubscriptionPlanResult> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to create subscription plan");

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

            return new CreateSubscriptionPlanResult
            {
                Id = subscriptionPlan.Id,
                Name = subscriptionPlan.Name,
                Description = subscriptionPlan.Description,
                Amount = subscriptionPlan.Price.Amount,
                Currency = subscriptionPlan.Price.Currency,
                BillingPeriod = subscriptionPlan.BillingPeriod.ToString(),
                TrialPeriodDays = subscriptionPlan.TrialPeriodDays,
                IsActive = subscriptionPlan.IsActive,
                IsPublic = subscriptionPlan.IsPublic,
                SortOrder = subscriptionPlan.SortOrder,
                CreatedAt = subscriptionPlan.CreatedAt
            };
        }
    }
}
