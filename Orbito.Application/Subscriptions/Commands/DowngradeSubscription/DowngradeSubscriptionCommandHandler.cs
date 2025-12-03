using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.DowngradeSubscription
{
    public class DowngradeSubscriptionCommandHandler : IRequestHandler<DowngradeSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<DowngradeSubscriptionCommandHandler> _logger;

        public DowngradeSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<DowngradeSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(DowngradeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Downgrading subscription {SubscriptionId} to plan {NewPlanId} for client {ClientId}",
                request.SubscriptionId, request.NewPlanId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to downgrade subscription without tenant context");
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);
            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for client {ClientId}", request.SubscriptionId, request.ClientId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            if (!await _subscriptionService.CanDowngradeAsync(subscription, request.NewPlanId, cancellationToken))
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be downgraded to plan {NewPlanId}",
                    request.SubscriptionId, request.NewPlanId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotDowngrade);
            }

            var newPrice = Money.Create(request.NewAmount, request.Currency);
            var updatedSubscription = await _subscriptionService.ProcessSubscriptionChangeAsync(
                subscription, request.NewPlanId, newPrice, cancellationToken);

            _logger.LogInformation("Successfully downgraded subscription {SubscriptionId} to plan {NewPlanId}",
                request.SubscriptionId, request.NewPlanId);

            var dto = new SubscriptionDto
            {
                Id = updatedSubscription.Id,
                TenantId = updatedSubscription.TenantId.Value,
                ClientId = updatedSubscription.ClientId,
                PlanId = updatedSubscription.PlanId,
                Status = updatedSubscription.Status.ToString(),
                Amount = updatedSubscription.CurrentPrice.Amount,
                Currency = updatedSubscription.CurrentPrice.Currency,
                BillingPeriodValue = updatedSubscription.BillingPeriod.Value,
                BillingPeriodType = updatedSubscription.BillingPeriod.Type.ToString(),
                StartDate = updatedSubscription.StartDate,
                EndDate = updatedSubscription.EndDate,
                NextBillingDate = updatedSubscription.NextBillingDate,
                IsInTrial = updatedSubscription.IsInTrial,
                TrialEndDate = updatedSubscription.TrialEndDate,
                ExternalSubscriptionId = updatedSubscription.ExternalSubscriptionId,
                CreatedAt = updatedSubscription.CreatedAt,
                CancelledAt = updatedSubscription.CancelledAt,
                UpdatedAt = updatedSubscription.UpdatedAt
            };

            return Result.Success(dto);
        }
    }
}
