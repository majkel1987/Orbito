using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Handler for cancelling a subscription.
/// </summary>
public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

        public CancelSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<CancelSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancelling subscription {SubscriptionId} for client {ClientId}", request.SubscriptionId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to cancel subscription without tenant context");
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);
            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for client {ClientId}", request.SubscriptionId, request.ClientId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            if (!subscription.CanBeCancelled())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be cancelled. Current status: {Status}",
                    request.SubscriptionId, subscription.Status);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotBeCancelled);
            }

            subscription.Cancel();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        _logger.LogInformation("Successfully cancelled subscription {SubscriptionId}", request.SubscriptionId);

        return Result.Success(SubscriptionMapper.ToDto(subscription));
    }
}
