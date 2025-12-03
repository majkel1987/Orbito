using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public class ResumeSubscriptionCommandHandler : IRequestHandler<ResumeSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<ResumeSubscriptionCommandHandler> _logger;

        public ResumeSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<ResumeSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(ResumeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resuming subscription {SubscriptionId} for client {ClientId}", request.SubscriptionId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to resume subscription without tenant context");
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);
            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for client {ClientId}", request.SubscriptionId, request.ClientId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            if (!subscription.CanBeResumed())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be resumed. Current status: {Status}",
                    request.SubscriptionId, subscription.Status);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotResume);
            }

            subscription.Resume();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully resumed subscription {SubscriptionId}", request.SubscriptionId);

            var subscriptionDto = new SubscriptionDto
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

            return Result.Success(subscriptionDto);
        }
    }
}
