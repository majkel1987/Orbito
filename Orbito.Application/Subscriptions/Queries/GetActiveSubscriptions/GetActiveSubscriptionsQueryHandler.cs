using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions
{
    public class GetActiveSubscriptionsQueryHandler : IRequestHandler<GetActiveSubscriptionsQuery, Result<Common.Models.PaginatedList<SubscriptionDto>>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<GetActiveSubscriptionsQueryHandler> _logger;

        public GetActiveSubscriptionsQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<GetActiveSubscriptionsQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<Common.Models.PaginatedList<SubscriptionDto>>> Handle(GetActiveSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all subscriptions with pagination {PageNumber}/{PageSize}",
                request.PageNumber, request.PageSize);

            // SECURITY: Verify tenant context before querying
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("GetActiveSubscriptions attempted without tenant context");
                return Result.Failure<Common.Models.PaginatedList<SubscriptionDto>>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURE: Explicitly pass TenantId to ensure proper isolation
            var tenantId = _tenantContext.CurrentTenantId;
            var subscriptions = await _subscriptionRepository.GetSubscriptionsForTenantAsync(tenantId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                subscriptions = subscriptions.Where(s =>
                    s.Client?.CompanyName?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Client?.DirectEmail?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Plan?.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }

            var totalCount = subscriptions.Count();
            var pagedSubscriptions = subscriptions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var subscriptionDtos = pagedSubscriptions.Select(s => new SubscriptionDto
            {
                Id = s.Id,
                TenantId = s.TenantId.Value,
                ClientId = s.ClientId,
                PlanId = s.PlanId,
                Status = s.Status.ToString(),
                Amount = s.CurrentPrice.Amount,
                Currency = s.CurrentPrice.Currency,
                BillingPeriodValue = s.BillingPeriod.Value,
                BillingPeriodType = s.BillingPeriod.Type.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                IsInTrial = s.IsInTrial,
                TrialEndDate = s.TrialEndDate,
                ExternalSubscriptionId = s.ExternalSubscriptionId,
                CreatedAt = s.CreatedAt,
                CancelledAt = s.CancelledAt,
                UpdatedAt = s.UpdatedAt,
                // Client and Plan details
                ClientCompanyName = s.Client?.CompanyName,
                ClientEmail = s.Client?.DirectEmail,
                ClientFirstName = s.Client?.DirectFirstName,
                ClientLastName = s.Client?.DirectLastName,
                PlanName = s.Plan?.Name,
                PlanDescription = s.Plan?.Description
            }).ToList();

            var paginatedList = new Common.Models.PaginatedList<SubscriptionDto>(
                subscriptionDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} subscriptions", subscriptionDtos.Count);

            return Result.Success(paginatedList);
        }
    }
}
