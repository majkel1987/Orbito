namespace Orbito.Application.Subscriptions.Commands.UpgradeSubscription
{
    public record UpgradeSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public Guid NewPlanId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }

        public static UpgradeSubscriptionResult SuccessResult(Guid subscriptionId, Guid newPlanId, string status)
        {
            return new UpgradeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                Status = status,
                Message = "Subscription upgraded successfully",
                Success = true
            };
        }

        public static UpgradeSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new UpgradeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
