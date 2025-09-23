namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public record RenewSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }
        public DateTime NextBillingDate { get; init; }

        public static RenewSubscriptionResult SuccessResult(Guid subscriptionId, string status, DateTime nextBillingDate)
        {
            return new RenewSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Status = status,
                Message = "Subscription renewed successfully",
                Success = true,
                NextBillingDate = nextBillingDate
            };
        }

        public static RenewSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new RenewSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
