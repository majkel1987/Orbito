using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription
{
    public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, SuspendSubscriptionResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<SuspendSubscriptionCommandHandler> _logger;

        public SuspendSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<SuspendSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<SuspendSubscriptionResult> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Suspending subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return SuspendSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!subscription.CanBeSuspended())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be suspended. Current status: {Status}", 
                    request.SubscriptionId, subscription.Status);
                return SuspendSubscriptionResult.FailureResult(request.SubscriptionId, 
                    $"Subscription cannot be suspended. Current status: {subscription.Status}");
            }

            subscription.Suspend();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully suspended subscription {SubscriptionId}", request.SubscriptionId);

            return SuspendSubscriptionResult.SuccessResult(request.SubscriptionId, subscription.Status.ToString());
        }
    }
}
