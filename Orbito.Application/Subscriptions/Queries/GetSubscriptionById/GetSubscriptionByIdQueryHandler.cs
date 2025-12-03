using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionById
{
    public class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<GetSubscriptionByIdQueryHandler> _logger;

        public GetSubscriptionByIdQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<GetSubscriptionByIdQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subscription {SubscriptionId} with details: {IncludeDetails}",
                request.SubscriptionId, request.IncludeDetails);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("No tenant context for subscription query {SubscriptionId}", request.SubscriptionId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            var subscription = request.IncludeDetails
                ? await _subscriptionRepository.GetByIdWithDetailsAsync(request.SubscriptionId, cancellationToken)
                : await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            // SECURITY: Verify tenant ownership
            if (subscription.TenantId != tenantId)
            {
                _logger.LogWarning("Cross-tenant access attempt: Subscription {SubscriptionId} does not belong to tenant {TenantId}",
                    request.SubscriptionId, tenantId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.CrossTenantAccess);
            }

            var dto = new SubscriptionDto
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId.Value,
                ClientId = subscription.ClientId,
                PlanId = subscription.PlanId,
                Status = subscription.Status.ToString(),
                Amount = subscription.CurrentPrice.Amount,
                Currency = subscription.CurrentPrice.Currency,
                BillingPeriodValue = subscription.BillingPeriod.Value,
                BillingPeriodType = subscription.BillingPeriod.Type.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextBillingDate = subscription.NextBillingDate,
                IsInTrial = subscription.IsInTrial,
                TrialEndDate = subscription.TrialEndDate,
                ExternalSubscriptionId = subscription.ExternalSubscriptionId,
                CreatedAt = subscription.CreatedAt,
                CancelledAt = subscription.CancelledAt,
                UpdatedAt = subscription.UpdatedAt
            };

            if (request.IncludeDetails)
            {
                dto = dto with
                {
                    ClientCompanyName = subscription.Client?.CompanyName,
                    ClientEmail = subscription.Client?.DirectEmail,
                    ClientFirstName = subscription.Client?.DirectFirstName,
                    ClientLastName = subscription.Client?.DirectLastName,
                    PlanName = subscription.Plan?.Name,
                    PlanDescription = subscription.Plan?.Description,
                    PaymentCount = subscription.Payments?.Count ?? 0,
                    TotalPaid = subscription.Payments?.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Sum(p => p.Amount.Amount) ?? 0,
                    LastPaymentDate = subscription.Payments?.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Max(p => p.ProcessedAt)
                };
            }

            _logger.LogInformation("Successfully retrieved subscription {SubscriptionId}", request.SubscriptionId);

            return Result.Success(dto);
        }
    }
}
