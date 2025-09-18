using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Infrastructure.Data;
using System.Linq.Expressions;

namespace Orbito.Infrastructure.Persistance
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            return entry.Entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return await _dbSet.CountAsync(cancellationToken);
            
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
