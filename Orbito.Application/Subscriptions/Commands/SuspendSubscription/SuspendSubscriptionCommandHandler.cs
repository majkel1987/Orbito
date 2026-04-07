using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription;

/// <summary>
/// Handler for suspending a subscription.
/// </summary>
public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<SuspendSubscriptionCommandHandler> _logger;

        public SuspendSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ITenantContext tenantContext,
            ILogger<SuspendSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task<Result<SubscriptionDto>> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suspending subscription {SubscriptionId} for client {ClientId}", request.SubscriptionId, request.ClientId);

            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                _logger.LogWarning("Attempted to suspend subscription without tenant context");
                return Result.Failure<SubscriptionDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // SECURITY: Use ForClient method to verify ownership
            var subscription = await _subscriptionRepository.GetByIdForClientAsync(request.SubscriptionId, request.ClientId, cancellationToken);
            if (subscription == null || subscription.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for client {ClientId}", request.SubscriptionId, request.ClientId);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.NotFound);
            }

            if (!subscription.CanBeSuspended())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be suspended. Current status: {Status}",
                    request.SubscriptionId, subscription.Status);
                return Result.Failure<SubscriptionDto>(DomainErrors.Subscription.CannotSuspend);
            }

            subscription.Suspend();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

        _logger.LogInformation("Successfully suspended subscription {SubscriptionId}", request.SubscriptionId);

        return Result.Success(SubscriptionMapper.ToDto(subscription));
    }
}
