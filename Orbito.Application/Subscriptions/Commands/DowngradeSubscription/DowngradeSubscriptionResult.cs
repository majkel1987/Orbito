namespace Orbito.Application.Subscriptions.Commands.DowngradeSubscription
{
    public record DowngradeSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public Guid NewPlanId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }

        public static DowngradeSubscriptionResult SuccessResult(Guid subscriptionId, Guid newPlanId, string status)
        {
            return new DowngradeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                Status = status,
                Message = "Subscription downgraded successfully",
                Success = true
            };
        }

        public static DowngradeSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new DowngradeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
