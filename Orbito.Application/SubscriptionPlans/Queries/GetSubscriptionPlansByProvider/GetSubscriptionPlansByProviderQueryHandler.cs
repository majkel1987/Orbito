using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public class GetSubscriptionPlansByProviderQueryHandler : IRequestHandler<GetSubscriptionPlansByProviderQuery, SubscriptionPlansListDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public GetSubscriptionPlansByProviderQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<SubscriptionPlansListDto> Handle(GetSubscriptionPlansByProviderQuery request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required to get subscription plans");

            var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.ActiveOnly,
                request.PublicOnly,
                cancellationToken);

            var totalCount = await _unitOfWork.SubscriptionPlans.GetCountAsync(
                request.SearchTerm,
                request.ActiveOnly,
                request.PublicOnly,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var items = subscriptionPlans.Select(plan => new SubscriptionPlanListItemDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                Amount = plan.Price.Amount,
                Currency = plan.Price.Currency,
                BillingPeriod = plan.BillingPeriod.ToString(),
                TrialPeriodDays = plan.TrialPeriodDays,
                IsActive = plan.IsActive,
                IsPublic = plan.IsPublic,
                SortOrder = plan.SortOrder,
                CreatedAt = plan.CreatedAt,
                ActiveSubscriptionsCount = plan.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active),
                TotalSubscriptionsCount = plan.Subscriptions.Count
            }).ToList();

            return new SubscriptionPlansListDto
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };
        }
    }
}
