using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, RenewSubscriptionResult>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<RenewSubscriptionCommandHandler> _logger;

        public RenewSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            ISubscriptionRepository subscriptionRepository,
            ILogger<RenewSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<RenewSubscriptionResult> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Renewing subscription {SubscriptionId}", request.SubscriptionId);

            var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return RenewSubscriptionResult.FailureResult(request.SubscriptionId, "Subscription not found");
            }

            var amount = Money.Create(request.Amount, request.Currency);
            var success = await _subscriptionService.ProcessPaymentAsync(
                request.SubscriptionId, amount, request.ExternalPaymentId, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed to process payment for subscription {SubscriptionId}", request.SubscriptionId);
                return RenewSubscriptionResult.FailureResult(request.SubscriptionId, "Failed to process payment");
            }

            // Get updated subscription to get new billing date
            var updatedSubscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

            _logger.LogInformation("Successfully renewed subscription {SubscriptionId}", request.SubscriptionId);

            return RenewSubscriptionResult.SuccessResult(
                request.SubscriptionId, 
                updatedSubscription!.Status.ToString(), 
                updatedSubscription.NextBillingDate);
        }
    }
}
