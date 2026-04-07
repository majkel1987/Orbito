using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Interfaces
{
    public interface ISubscriptionService
    {
        // Business logic methods
        Task<DateTime> CalculateNextBillingDateAsync(Subscription subscription, CancellationToken cancellationToken = default);
        Task<bool> CanUpgradeAsync(Subscription subscription, Guid newPlanId, CancellationToken cancellationToken = default);
        Task<bool> CanDowngradeAsync(Subscription subscription, Guid newPlanId, CancellationToken cancellationToken = default);
        Task<Subscription> ProcessSubscriptionChangeAsync(Subscription subscription, Guid newPlanId, Money newPrice, CancellationToken cancellationToken = default);
        Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default);
        Task<Subscription> CreateSubscriptionAsync(Guid clientId, Guid planId, Money price, BillingPeriod billingPeriod, int trialDays = 0, CancellationToken cancellationToken = default);
        Task<bool> ProcessPaymentAsync(Guid subscriptionId, Money amount, string? externalPaymentId = null, CancellationToken cancellationToken = default);
        Task ProcessRecurringPaymentsAsync(DateTime billingDate, CancellationToken cancellationToken = default);
    }
}
