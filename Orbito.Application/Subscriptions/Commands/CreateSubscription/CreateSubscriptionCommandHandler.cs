using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionResult>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IClientRepository _clientRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

        public CreateSubscriptionCommandHandler(
            ISubscriptionService subscriptionService,
            IClientRepository clientRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            ILogger<CreateSubscriptionCommandHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _clientRepository = clientRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _logger = logger;
        }

        public async Task<CreateSubscriptionResult> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating subscription for client {ClientId} with plan {PlanId}", 
                request.ClientId, request.PlanId);

            // Validate client exists
            var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {request.ClientId} not found");
            }

            // Validate plan exists and is active
            var plan = await _subscriptionPlanRepository.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null)
            {
                throw new InvalidOperationException($"Plan with ID {request.PlanId} not found");
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException($"Plan with ID {request.PlanId} is not active");
            }

            // Create value objects
            var price = Money.Create(request.Amount, request.Currency);
            var billingPeriodType = Enum.Parse<BillingPeriodType>(request.BillingPeriodType, true);
            var billingPeriod = BillingPeriod.Create(request.BillingPeriodValue, billingPeriodType);

            // Create subscription
            var subscription = await _subscriptionService.CreateSubscriptionAsync(
                request.ClientId,
                request.PlanId,
                price,
                billingPeriod,
                request.TrialDays,
                cancellationToken);

            _logger.LogInformation("Successfully created subscription {SubscriptionId} for client {ClientId}", 
                subscription.Id, request.ClientId);

            return CreateSubscriptionResult.FromSubscription(subscription);
        }
    }
}   
