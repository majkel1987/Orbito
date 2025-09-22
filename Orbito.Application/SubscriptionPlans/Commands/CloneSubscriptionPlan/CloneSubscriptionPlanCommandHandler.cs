using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan
{
    public class CloneSubscriptionPlanCommandHandler : IRequestHandler<CloneSubscriptionPlanCommand, CloneSubscriptionPlanResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public CloneSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<CloneSubscriptionPlanResult> Handle(CloneSubscriptionPlanCommand request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to clone subscription plan");

            var originalPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
            if (originalPlan == null)
                throw new InvalidOperationException($"Subscription plan with ID {request.Id} not found");

            // Create new plan based on original
            var clonedPlan = SubscriptionPlan.Create(
                _tenantContext.CurrentTenantId!,
                request.NewName,
                request.NewAmount ?? originalPlan.Price.Amount,
                request.NewCurrency ?? originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type,
                request.NewDescription ?? originalPlan.Description,
                originalPlan.TrialDays,
                originalPlan.TrialPeriodDays,
                originalPlan.FeaturesJson,
                originalPlan.LimitationsJson,
                request.NewSortOrder ?? originalPlan.SortOrder);

            // Set status and visibility
            if (request.IsActive)
                clonedPlan.Activate();
            else
                clonedPlan.Deactivate();

            clonedPlan.UpdateVisibility(request.IsPublic);

            await _unitOfWork.SubscriptionPlans.AddAsync(clonedPlan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CloneSubscriptionPlanResult
            {
                Id = clonedPlan.Id,
                Name = clonedPlan.Name,
                Description = clonedPlan.Description,
                Amount = clonedPlan.Price.Amount,
                Currency = clonedPlan.Price.Currency,
                BillingPeriod = clonedPlan.BillingPeriod.ToString(),
                TrialPeriodDays = clonedPlan.TrialPeriodDays,
                IsActive = clonedPlan.IsActive,
                IsPublic = clonedPlan.IsPublic,
                SortOrder = clonedPlan.SortOrder,
                CreatedAt = clonedPlan.CreatedAt,
                OriginalPlanId = originalPlan.Id
            };
        }
    }
}
