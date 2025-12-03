namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Response for retry failed payment command
    /// </summary>
    public record RetryFailedPaymentResponse
    {
        /// <summary>
        /// ID of the created retry schedule
        /// </summary>
        public required Guid RetryScheduleId { get; init; }

        /// <summary>
        /// When the next retry attempt will be made
        /// </summary>
        public required DateTime NextAttemptAt { get; init; }

        /// <summary>
        /// Current attempt number
        /// </summary>
        public required int AttemptNumber { get; init; }

        /// <summary>
        /// Maximum number of attempts allowed
        /// </summary>
        public required int MaxAttempts { get; init; }
    }
}

