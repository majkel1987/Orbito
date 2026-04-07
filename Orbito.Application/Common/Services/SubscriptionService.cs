using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly IDateTime _dateTime;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IClientRepository clientRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            IDateTime dateTime,
            ILogger<SubscriptionService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _clientRepository = clientRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _dateTime = dateTime;
            _logger = logger;
        }

        public Task<DateTime> CalculateNextBillingDateAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            var currentDate = _dateTime.UtcNow;
            
            if (subscription.IsInTrial && subscription.TrialEndDate.HasValue)
            {
                return Task.FromResult(subscription.TrialEndDate.Value);
            }

            return Task.FromResult(subscription.BillingPeriod.GetNextBillingDate(currentDate));
        }

        public async Task<bool> CanUpgradeAsync(Subscription subscription, Guid newPlanId, CancellationToken cancellationToken = default)
        {
            if (!subscription.CanBeUpgraded())
            {
                return false;
            }

            var newPlan = await _subscriptionPlanRepository.GetByIdAsync(newPlanId, cancellationToken);
            if (newPlan == null || !newPlan.IsActive)
            {
                return false;
            }

            // Check if new plan price is higher than current
            return newPlan.Price.Amount > subscription.CurrentPrice.Amount;
        }

        public async Task<bool> CanDowngradeAsync(Subscription subscription, Guid newPlanId, CancellationToken cancellationToken = default)
        {
            if (!subscription.CanBeDowngraded())
            {
                return false;
            }

            var newPlan = await _subscriptionPlanRepository.GetByIdAsync(newPlanId, cancellationToken);
            if (newPlan == null || !newPlan.IsActive)
            {
                return false;
            }

            // Check if new plan price is lower than current
            return newPlan.Price.Amount < subscription.CurrentPrice.Amount;
        }

        public async Task<Subscription> ProcessSubscriptionChangeAsync(Subscription subscription, Guid newPlanId, Money newPrice, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing subscription change for subscription {SubscriptionId} to plan {NewPlanId}", 
                subscription.Id, newPlanId);

            subscription.ChangePlan(newPlanId, newPrice);
            subscription.UpdateNextBillingDate();

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully processed subscription change for subscription {SubscriptionId}", 
                subscription.Id);

            return subscription;
        }

        public async Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);
            if (client == null)
            {
                return false;
            }

            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId, cancellationToken);
            if (plan == null || !plan.IsActive)
            {
                return false;
            }

            return await _subscriptionRepository.CanClientSubscribeToPlanAsync(clientId, planId, cancellationToken);
        }

        public async Task<Subscription> CreateSubscriptionAsync(Guid clientId, Guid planId, Money price, BillingPeriod billingPeriod, int trialDays = 0, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating subscription for client {ClientId} with plan {PlanId}", 
                clientId, planId);

            var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);
            if (client == null)
            {
                throw new InvalidOperationException($"Client with ID {clientId} not found");
            }

            var plan = await _subscriptionPlanRepository.GetByIdAsync(planId, cancellationToken);
            if (plan == null)
            {
                throw new InvalidOperationException($"Plan with ID {planId} not found");
            }

            if (!await CanClientSubscribeToPlanAsync(clientId, planId, cancellationToken))
            {
                throw new InvalidOperationException($"Client {clientId} cannot subscribe to plan {planId}");
            }

            var subscription = Subscription.Create(
                client.TenantId,
                clientId,
                planId,
                price,
                billingPeriod,
                trialDays);

            var createdSubscription = await _subscriptionRepository.AddAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully created subscription {SubscriptionId} for client {ClientId}", 
                createdSubscription.Id, clientId);

            return createdSubscription;
        }

        public async Task<bool> ProcessPaymentAsync(Guid subscriptionId, Money amount, string? externalPaymentId = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing payment for subscription {SubscriptionId} with amount {Amount}", 
                subscriptionId, amount);

            var subscription = await _subscriptionRepository.GetByIdWithDetailsAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found for payment processing", subscriptionId);
                return false;
            }

            var payment = Payment.Create(
                subscription.TenantId,
                subscriptionId,
                subscription.ClientId,
                amount,
                externalPaymentId);

            // In a real implementation, you would integrate with a payment processor here
            // For now, we'll simulate a successful payment
                payment.MarkAsCompleted();

            // Update subscription next billing date
            subscription.UpdateNextBillingDate();

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            _logger.LogInformation("Successfully processed payment for subscription {SubscriptionId}", subscriptionId);
            return true;
        }


        public async Task ProcessRecurringPaymentsAsync(DateTime billingDate, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing recurring payments for billing date {BillingDate}", billingDate);

            var subscriptionsForBilling = await _subscriptionRepository.GetSubscriptionsForBillingAsync(billingDate, cancellationToken);

            foreach (var subscription in subscriptionsForBilling)
            {
                try
                {
                    var success = await ProcessPaymentAsync(subscription.Id, subscription.CurrentPrice, cancellationToken: cancellationToken);
                    
                    if (success)
                    {
                        _logger.LogInformation("Successfully processed recurring payment for subscription {SubscriptionId}", subscription.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to process recurring payment for subscription {SubscriptionId}", subscription.Id);
                        subscription.MarkAsPastDue();
                        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing recurring payment for subscription {SubscriptionId}", subscription.Id);
                    subscription.MarkAsPastDue();
                    await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
                }
            }

            _logger.LogInformation("Processed recurring payments for {Count} subscriptions", subscriptionsForBilling.Count());
        }
    }
}
