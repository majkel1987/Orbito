using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan
{
    public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, UpdateSubscriptionPlanResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public UpdateSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<UpdateSubscriptionPlanResult> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to update subscription plan");

            var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (subscriptionPlan == null)
                throw new InvalidOperationException($"Subscription plan with ID {request.Id} not found");

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

            return new UpdateSubscriptionPlanResult
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
                UpdatedAt = subscriptionPlan.UpdatedAt ?? subscriptionPlan.CreatedAt
            };
        }
    }
}
