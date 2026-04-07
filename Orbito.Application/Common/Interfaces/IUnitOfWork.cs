using System.Data;
using Orbito.Application.Common.Models;
using Orbito.Domain.Interfaces;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern for managing database transactions and repositories
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Whether there is an active transaction
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Creates an execution strategy compatible with retry logic and manual transactions
    /// </summary>
    /// <returns>Execution strategy for resilient database operations</returns>
    IExecutionStrategy CreateExecutionStrategy();

    /// <summary>
    /// Begins a new transaction with default isolation level
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction with specified isolation level
    /// </summary>
    /// <param name="isolationLevel">Transaction isolation level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<Result> RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction (backward compatibility)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [Obsolete("Use CommitAsync instead. Will be removed in v2.0.")]
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction (backward compatibility)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [Obsolete("Use RollbackAsync instead. Will be removed in v2.0.")]
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Repository instance</returns>
    IRepository<T> GetRepository<T>() where T : class;

    // Specific repositories
    IProviderRepository Providers { get; }
    IClientRepository Clients { get; }
    ISubscriptionRepository Subscriptions { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IPaymentRepository Payments { get; }
    IPaymentMethodRepository PaymentMethods { get; }
    IPaymentRetryRepository PaymentRetries { get; }
    IWebhookLogRepository WebhookLogs { get; }
    IEmailNotificationRepository EmailNotifications { get; }
    ITeamMemberRepository TeamMembers { get; }
}
