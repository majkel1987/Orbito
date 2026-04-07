using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using EfExecutionStrategy = Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy;
using IExecutionStrategy = Orbito.Application.Common.Interfaces.IExecutionStrategy;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Adapter that wraps EF Core IExecutionStrategy to Application layer IExecutionStrategy.
/// Provides abstraction to prevent EF Core dependencies leaking into Application layer.
/// </summary>
internal sealed class ExecutionStrategyAdapter : IExecutionStrategy
{
    private readonly EfExecutionStrategy _efStrategy;

    public ExecutionStrategyAdapter(EfExecutionStrategy efStrategy)
    {
        _efStrategy = efStrategy ?? throw new ArgumentNullException(nameof(efStrategy));
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        return await _efStrategy.ExecuteAsync(
            state: operation,
            operation: static (context, state, ct) => state(),
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await _efStrategy.ExecuteAsync(
            state: operation,
            operation: static async (context, state, ct) =>
            {
                await state();
                return true;
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }
}
