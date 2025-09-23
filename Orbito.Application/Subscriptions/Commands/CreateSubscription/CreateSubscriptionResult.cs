using Orbito.Domain.Entities;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription
{
    public record CreateSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime NextBillingDate { get; init; }
        public bool IsInTrial { get; init; }
        public DateTime? TrialEndDate { get; init; }
        public string Message { get; init; } = string.Empty;

        public static CreateSubscriptionResult FromSubscription(Subscription subscription)
        {
            return new CreateSubscriptionResult
            {
                SubscriptionId = subscription.Id,
                ClientId = subscription.ClientId,
                PlanId = subscription.PlanId,
                Status = subscription.Status.ToString(),
                StartDate = subscription.StartDate,
                NextBillingDate = subscription.NextBillingDate,
                IsInTrial = subscription.IsInTrial,
                TrialEndDate = subscription.TrialEndDate,
                Message = "Subscription created successfully"
            };
        }
    }
}
