using Microsoft.EntityFrameworkCore.Storage;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Adapter that wraps EF Core IDbContextTransaction to Application layer ITransactionScope.
/// Provides abstraction to prevent EF Core dependencies leaking into Application layer.
/// </summary>
internal sealed class TransactionScopeAdapter : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    public TransactionScopeAdapter(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
}
