using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.UpgradeSubscription
{
    public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, UpgradeSubscriptionResult>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<UpgradeSubscriptionCommandHandler> _logger;

        public UpgradeSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            ISubscriptionRepository subscriptionRepository,
            ILogger<UpgradeSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<UpgradeSubscriptionResult> Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Upgrading subscription {SubscriptionId} to plan {NewPlanId}", 
                request.SubscriptionId, request.NewPlanId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return UpgradeSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            if (!await _subscriptionService.CanUpgradeAsync(subscription, request.NewPlanId, cancellationToken))
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be upgraded to plan {NewPlanId}", 
                    request.SubscriptionId, request.NewPlanId);
                return UpgradeSubscriptionResult.FailureResult(request.SubscriptionId, 
                    "Subscription cannot be upgraded to the specified plan");
            }

            var newPrice = Money.Create(request.NewAmount, request.Currency);
            var updatedSubscription = await _subscriptionService.ProcessSubscriptionChangeAsync(
                subscription, request.NewPlanId, newPrice, cancellationToken);

            _logger.LogInformation("Successfully upgraded subscription {SubscriptionId} to plan {NewPlanId}", 
                request.SubscriptionId, request.NewPlanId);

            return UpgradeSubscriptionResult.SuccessResult(request.SubscriptionId, request.NewPlanId, updatedSubscription.Status.ToString());
        }
    }
}
