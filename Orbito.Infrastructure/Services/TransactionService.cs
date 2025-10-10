using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Services;

/// <summary>
/// Service for managing database transactions with proper rollback handling
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(ApplicationDbContext context, ILogger<TransactionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogDebug("Starting database transaction");
                
                var result = await operation();
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogDebug("Database transaction committed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in database transaction, rolling back");
                
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogDebug("Database transaction rolled back successfully");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                }
                
                throw;
            }
        });
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return Task.CompletedTask;
        }, cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(IEnumerable<Func<Task>> operations, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogDebug("Starting database transaction with {OperationCount} operations", operations.Count());
                
                foreach (var operation in operations)
                {
                    await operation();
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogDebug("Database transaction with {OperationCount} operations committed successfully", operations.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in database transaction with {OperationCount} operations, rolling back", operations.Count());
                
                try
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogDebug("Database transaction rolled back successfully");
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                }
                
                throw;
            }
        });
    }
}

