using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public class GetSubscriptionPlansByProviderQueryHandler : IRequestHandler<GetSubscriptionPlansByProviderQuery, Domain.Common.Result<PaginatedList<SubscriptionPlanListItemDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<GetSubscriptionPlansByProviderQueryHandler> _logger;

        public GetSubscriptionPlansByProviderQueryHandler(
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            ILogger<GetSubscriptionPlansByProviderQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Domain.Common.Result<PaginatedList<SubscriptionPlanListItemDto>>> Handle(GetSubscriptionPlansByProviderQuery request, CancellationToken cancellationToken)
        {
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to get subscription plans without tenant context");
                return Domain.Common.Result.Failure<PaginatedList<SubscriptionPlanListItemDto>>(DomainErrors.Tenant.NoTenantContext);
            }

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

            var paginatedList = new PaginatedList<SubscriptionPlanListItemDto>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} subscription plans for page {PageNumber}", items.Count, request.PageNumber);

            return Domain.Common.Result.Success(paginatedList);
        }
    }
}
