using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription;

/// <summary>
/// Handler for activating a subscription.
/// </summary>
public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<ActivateSubscriptionCommandHandler> _logger;

        public ActivateSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<ActivateSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating subscription {SubscriptionId} for client {ClientId}", request.SubscriptionId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to activate subscription without tenant context");
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
                _logger.LogWarning("Subscription {SubscriptionId} cannot be activated. Current status: {Status}",
                    request.SubscriptionId, subscription.Status);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.AlreadyActive);
            }

            subscription.Activate();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        _logger.LogInformation("Successfully activated subscription {SubscriptionId}", request.SubscriptionId);

        return Result.Success(SubscriptionMapper.ToDto(subscription));
    }
}
