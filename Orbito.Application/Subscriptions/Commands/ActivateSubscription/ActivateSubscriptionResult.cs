namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription
{
    public record ActivateSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }

        public static ActivateSubscriptionResult SuccessResult(Guid subscriptionId, string status)
        {
            return new ActivateSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Status = status,
                Message = "Subscription activated successfully",
                Success = true
            };
        }

        public static ActivateSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new ActivateSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
