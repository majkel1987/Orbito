using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.DowngradeSubscription
{
    public class DowngradeSubscriptionCommandHandler : IRequestHandler<DowngradeSubscriptionCommand, DowngradeSubscriptionResult>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<DowngradeSubscriptionCommandHandler> _logger;

        public DowngradeSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            ISubscriptionRepository subscriptionRepository,
            ILogger<DowngradeSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<DowngradeSubscriptionResult> Handle(DowngradeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Downgrading subscription {SubscriptionId} to plan {NewPlanId}", 
                request.SubscriptionId, request.NewPlanId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return DowngradeSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!await _subscriptionService.CanDowngradeAsync(subscription, request.NewPlanId, cancellationToken))
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be downgraded to plan {NewPlanId}", 
                    request.SubscriptionId, request.NewPlanId);
                return DowngradeSubscriptionResult.FailureResult(request.SubscriptionId, 
                    "Subscription cannot be downgraded to the specified plan");
            }

            var newPrice = Money.Create(request.NewAmount, request.Currency);
            var updatedSubscription = await _subscriptionService.ProcessSubscriptionChangeAsync(
                subscription, request.NewPlanId, newPrice, cancellationToken);

            _logger.LogInformation("Successfully downgraded subscription {SubscriptionId} to plan {NewPlanId}", 
                request.SubscriptionId, request.NewPlanId);

            return DowngradeSubscriptionResult.SuccessResult(request.SubscriptionId, request.NewPlanId, updatedSubscription.Status.ToString());
        }
    }
}
