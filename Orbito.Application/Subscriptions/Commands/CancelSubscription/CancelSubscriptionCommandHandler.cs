using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Commands.CancelSubscription
{
    public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

        public CancelSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<CancelSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<CancelSubscriptionResult> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancelling subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return CancelSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!subscription.CanBeCancelled())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be cancelled. Current status: {Status}", 
                    request.SubscriptionId, subscription.Status);
                return CancelSubscriptionResult.FailureResult(request.SubscriptionId, 
                    $"Subscription cannot be cancelled. Current status: {subscription.Status}");
            }

            subscription.Cancel();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully cancelled subscription {SubscriptionId}", request.SubscriptionId);

            return CancelSubscriptionResult.SuccessResult(request.SubscriptionId, subscription.Status.ToString(), subscription.CancelledAt!.Value);
        }
    }
}
