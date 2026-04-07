namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Abstraction for database transaction scope.
/// Allows Application layer to manage transactions without EF Core dependency.
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing database transactions
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Executes an operation within a database transaction
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an operation within a database transaction with rollback on failure
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes multiple operations within a single database transaction
    /// </summary>
    /// <param name="operations">Operations to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(IEnumerable<Func<Task>> operations, CancellationToken cancellationToken = default);
}
