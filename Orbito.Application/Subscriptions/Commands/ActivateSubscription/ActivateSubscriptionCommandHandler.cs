using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription
{
    public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, ActivateSubscriptionResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<ActivateSubscriptionCommandHandler> _logger;

        public ActivateSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<ActivateSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<ActivateSubscriptionResult> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return ActivateSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!subscription.CanBeResumed())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be activated. Current status: {Status}", 
                    request.SubscriptionId, subscription.Status);
                return ActivateSubscriptionResult.FailureResult(request.SubscriptionId, 
                    $"Subscription cannot be activated. Current status: {subscription.Status}");
            }

            subscription.Activate();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully activated subscription {SubscriptionId}", request.SubscriptionId);

            return ActivateSubscriptionResult.SuccessResult(request.SubscriptionId, subscription.Status.ToString());
        }
    }
}
