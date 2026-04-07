using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository for managing payment retry schedules.
/// Provides CRUD and query operations for PaymentRetrySchedule entities.
/// </summary>
public interface IPaymentRetryRepository
{
    /// <summary>
    /// Gets all retry schedules that are due for processing.
    /// Includes:
    /// - Scheduled retries where NextAttemptAt &lt;= now
    /// - InProgress retries that are stuck (LastAttemptAt older than inProgressTimeout)
    /// </summary>
    /// <param name="now">Current time</param>
    /// <param name="inProgressTimeout">Timeout for stuck InProgress retries (default: 30 minutes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of due retry schedules</returns>
    Task<List<PaymentRetrySchedule>> GetDueRetriesAsync(DateTime now, TimeSpan? inProgressTimeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retry schedule by payment ID and client ID.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="clientId">Client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retry schedule or null if not found</returns>
    Task<PaymentRetrySchedule?> GetByPaymentIdAsync(Guid paymentId, Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retry schedule by ID for a specific client.
    /// SECURITY: Prevents cross-client data leak.
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="clientId">Client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retry schedule or null if not found</returns>
    Task<PaymentRetrySchedule?> GetByIdForClientAsync(Guid scheduleId, Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active retry schedule for a payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active retry schedule or null if not found</returns>
    Task<PaymentRetrySchedule?> GetActiveRetryByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active retry schedules for a payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active retry schedules</returns>
    Task<List<PaymentRetrySchedule>> GetActiveRetriesByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retry schedules for a payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of retry schedules</returns>
    Task<List<PaymentRetrySchedule>> GetRetrySchedulesByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next retry schedule for a payment.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next retry schedule or null if not found</returns>
    Task<PaymentRetrySchedule?> GetNextRetryByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a retry schedule as processing (optimistic concurrency).
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if marked successfully, false if already processing</returns>
    Task<bool> MarkAsProcessingAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new retry schedule.
    /// </summary>
    /// <param name="retrySchedule">Retry schedule to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(PaymentRetrySchedule retrySchedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing retry schedule.
    /// </summary>
    /// <param name="retrySchedule">Retry schedule to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(PaymentRetrySchedule retrySchedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queryable retry schedules for filtering.
    /// </summary>
    /// <param name="clientId">Client ID to filter by</param>
    /// <param name="status">Status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Queryable retry schedules</returns>
    Task<IQueryable<PaymentRetrySchedule>> GetScheduledRetriesQueryAsync(Guid? clientId = null, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets retry schedules by payment IDs for a specific client.
    /// SECURITY: Prevents cross-client data leak.
    /// </summary>
    /// <param name="paymentIds">List of payment IDs</param>
    /// <param name="clientId">Client ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of retry schedules</returns>
    Task<List<PaymentRetrySchedule>> GetRetrySchedulesByPaymentIdsAsync(List<Guid> paymentIds, Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction.
    /// Returns ITransactionScope from Application layer abstraction to avoid EF Core leak.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction scope abstraction</returns>
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
