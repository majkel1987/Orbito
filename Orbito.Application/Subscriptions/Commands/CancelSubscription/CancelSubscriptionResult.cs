namespace Orbito.Application.Subscriptions.Commands.CancelSubscription
{
    public record CancelSubscriptionResult
    {
        public Guid SubscriptionId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool Success { get; init; }
        public DateTime? CancelledAt { get; init; }

        public static CancelSubscriptionResult SuccessResult(Guid subscriptionId, string status, DateTime cancelledAt)
        {
            return new CancelSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Status = status,
                Message = "Subscription cancelled successfully",
                Success = true,
                CancelledAt = cancelledAt
            };
        }

        public static CancelSubscriptionResult FailureResult(Guid subscriptionId, string message)
        {
            return new CancelSubscriptionResult
            {
                SubscriptionId = subscriptionId,
                Message = message,
                Success = false
            };
        }
    }
}
