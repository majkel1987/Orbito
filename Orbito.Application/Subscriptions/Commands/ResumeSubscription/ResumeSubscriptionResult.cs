namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public record ResumeSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }

        public static ResumeSubscriptionResult SuccessResult(Guid subscriptionId, string status)
        {
            return new ResumeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Status = status,
                Message = "Subscription resumed successfully",
                Success = true
            };
        }

        public static ResumeSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new ResumeSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
