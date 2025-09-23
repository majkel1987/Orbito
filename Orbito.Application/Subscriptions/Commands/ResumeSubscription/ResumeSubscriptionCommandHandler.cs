using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public class ResumeSubscriptionCommandHandler : IRequestHandler<ResumeSubscriptionCommand, ResumeSubscriptionResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<ResumeSubscriptionCommandHandler> _logger;

        public ResumeSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<ResumeSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<ResumeSubscriptionResult> Handle(ResumeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resuming subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return ResumeSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!subscription.CanBeResumed())
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be resumed. Current status: {Status}", 
                    request.SubscriptionId, subscription.Status);
                return ResumeSubscriptionResult.FailureResult(request.SubscriptionId, 
                    $"Subscription cannot be resumed. Current status: {subscription.Status}");
            }

            subscription.Resume();
            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully resumed subscription {SubscriptionId}", request.SubscriptionId);

            return ResumeSubscriptionResult.SuccessResult(request.SubscriptionId, subscription.Status.ToString());
        }
    }
}
