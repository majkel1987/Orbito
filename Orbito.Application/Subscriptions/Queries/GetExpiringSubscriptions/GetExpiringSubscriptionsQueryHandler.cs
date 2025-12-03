using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions
{
    public class GetExpiringSubscriptionsQueryHandler : IRequestHandler<GetExpiringSubscriptionsQuery, Result<Common.Models.PaginatedList<SubscriptionDto>>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IDateTime _dateTime;
        private readonly ILogger<GetExpiringSubscriptionsQueryHandler> _logger;

        public GetExpiringSubscriptionsQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            IDateTime dateTime,
            ILogger<GetExpiringSubscriptionsQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task<Result<Common.Models.PaginatedList<SubscriptionDto>>> Handle(GetExpiringSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting expiring subscriptions within {Days} days", request.DaysBeforeExpiration);

            // SECURITY: Verify tenant context before querying
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("GetExpiringSubscriptions attempted without tenant context");
                return Result.Failure<Common.Models.PaginatedList<SubscriptionDto>>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURE: Explicitly pass TenantId to ensure proper isolation
            var tenantId = _tenantContext.CurrentTenantId;
            var checkDate = _dateTime.UtcNow;

            var expiringSubscriptions = await _subscriptionRepository.GetExpiringSubscriptionsForTenantAsync(
                tenantId,
                checkDate,
                request.DaysBeforeExpiration,
                cancellationToken);

            var totalCount = expiringSubscriptions.Count();
            var pagedSubscriptions = expiringSubscriptions
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

            _logger.LogInformation("Successfully retrieved {Count} expiring subscriptions", subscriptionDtos.Count);

            return Result.Success(paginatedList);
        }
    }
}
