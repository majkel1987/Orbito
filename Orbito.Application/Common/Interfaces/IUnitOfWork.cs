namespace Orbito.Application.Common.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        IRepository<T> GetRepository<T>() where T : class;
        
        // Specific repositories
        IProviderRepository Providers { get; }
        IClientRepository Clients { get; }
        ISubscriptionPlanRepository SubscriptionPlans { get; }
    }
}
