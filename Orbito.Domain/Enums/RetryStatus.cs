namespace Orbito.Domain.Enums
{
    /// <summary>
    /// Status of payment retry scheduling
    /// </summary>
    public enum RetryStatus
    {
        /// <summary>
        /// Retry is scheduled and waiting to be processed
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Retry is currently being processed
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Retry has been completed successfully
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Retry has been cancelled (payment succeeded or manually cancelled)
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Retry has failed after exhausting all attempts
        /// </summary>
        Failed = 5
    }
}
