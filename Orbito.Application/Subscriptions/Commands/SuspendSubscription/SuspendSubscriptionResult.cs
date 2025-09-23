namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription
{
    public record SuspendSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }

        public static SuspendSubscriptionResult SuccessResult(Guid subscriptionId, string status)
        {
            return new SuspendSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Status = status,
                Message = "Subscription suspended successfully",
                Success = true
            };
        }

        public static SuspendSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new SuspendSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
