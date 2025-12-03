using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<RenewSubscriptionCommandHandler> _logger;

        public RenewSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<RenewSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId} for client {ClientId}", request.SubscriptionId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to renew subscription without tenant context");
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);
            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for client {ClientId}", request.SubscriptionId, request.ClientId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            var amount = Money.Create(request.Amount, request.Currency);
            var success = await _subscriptionService.ProcessPaymentAsync(
                request.SubscriptionId, amount, request.ExternalPaymentId, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed to process payment for subscription {SubscriptionId}", request.SubscriptionId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotRenew);
            }

            // Get updated subscription to get new billing date (using same client verification)
            var updatedSubscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);

            _logger.LogInformation("Successfully renewed subscription {SubscriptionId}", request.SubscriptionId);

            var dto = new SubscriptionDto
            {
                Id = updatedSubscription!.Id,
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
