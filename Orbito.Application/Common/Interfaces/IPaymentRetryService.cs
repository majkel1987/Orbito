using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    /// <summary>
    /// Service for managing payment retry logic with exponential backoff
    /// </summary>
    public interface IPaymentRetryService
    {
        /// <summary>
        /// Schedules a retry for a failed payment
        /// </summary>
        /// <param name="paymentId">ID of the payment to retry</param>
        /// <param name="clientId">ID of the client (for security verification)</param>
        /// <param name="attemptNumber">Current attempt number (1-based)</param>
        /// <param name="errorReason">Reason for the failure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created retry schedule</returns>
        Task<PaymentRetrySchedule> ScheduleRetryAsync(Guid paymentId, Guid clientId, int attemptNumber, string errorReason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes all scheduled retries that are due
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of retries processed</returns>
        Task<int> ProcessScheduledRetriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates the next retry time using exponential backoff
        /// </summary>
        /// <param name="attemptNumber">Current attempt number</param>
        /// <returns>DateTime for the next retry attempt</returns>
        DateTime CalculateNextRetryTime(int attemptNumber);

        /// <summary>
        /// Cancels all scheduled retries for a payment
        /// </summary>
        /// <param name="paymentId">ID of the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of retries cancelled</returns>
        Task<int> CancelScheduledRetriesAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets retry schedules for a specific payment
        /// </summary>
        /// <param name="paymentId">ID of the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of retry schedules</returns>
        Task<List<PaymentRetrySchedule>> GetRetrySchedulesAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a payment has any active retry schedules
        /// </summary>
        /// <param name="paymentId">ID of the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if has active retries, false otherwise</returns>
        Task<bool> HasActiveRetriesAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next retry attempt for a payment
        /// </summary>
        /// <param name="paymentId">ID of the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Next retry schedule or null if none</returns>
        Task<PaymentRetrySchedule?> GetNextRetryAsync(Guid paymentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets active retries for multiple payments (batch operation)
        /// </summary>
        /// <param name="paymentIds">List of payment IDs</param>
        /// <param name="clientId">Client ID for security filtering</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary of payment ID to active retry schedule</returns>
        Task<Dictionary<Guid, PaymentRetrySchedule>> GetActiveRetriesForPaymentsAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Calculates the next attempt number for a payment
        /// </summary>
        /// <param name="paymentId">ID of the payment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Next attempt number (1-based)</returns>
        Task<int> CalculateNextAttemptNumberAsync(Guid paymentId, CancellationToken cancellationToken = default);
    }
}
