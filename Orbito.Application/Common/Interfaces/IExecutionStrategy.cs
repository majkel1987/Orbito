namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Abstraction for database execution strategy with retry logic.
/// Implementations handle transient failures and connection resilience.
/// </summary>
public interface IExecutionStrategy
{
    /// <summary>
    /// Executes the specified operation with retry logic for transient failures
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified operation with retry logic for transient failures
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}
