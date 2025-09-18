using Microsoft.EntityFrameworkCore;

namespace Orbito.Application.Common.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        IRepository<T> GetRepository<T>() where T : class;
    }

    public interface IRepository<T> where T : class
    {
        Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
        IQueryable<T> Query();
    }
}
