using MediatR;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Command to cancel a scheduled retry
    /// </summary>
    public class CancelRetryCommand : IRequest<CancelRetryResult>
    {
        /// <summary>
        /// ID of the retry schedule to cancel
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// ID of the client requesting the cancellation
        /// </summary>
        public Guid ClientId { get; set; }
    }

    /// <summary>
    /// Result of cancel retry command
    /// </summary>
    public class CancelRetryResult
    {
        /// <summary>
        /// Whether the cancellation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if cancellation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// ID of the cancelled retry schedule
        /// </summary>
        public Guid? ScheduleId { get; set; }

        /// <summary>
        /// Create a successful result
        /// </summary>
        public static CancelRetryResult SuccessResult(Guid scheduleId) => new()
        {
            Success = true,
            ScheduleId = scheduleId
        };

        /// <summary>
        /// Create a failed result
        /// </summary>
        public static CancelRetryResult FailureResult(string errorMessage) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
