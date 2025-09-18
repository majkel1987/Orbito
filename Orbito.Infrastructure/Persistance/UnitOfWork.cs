using Microsoft.EntityFrameworkCore.Storage;
using Orbito.Application.Common.Interfaces;
using Orbito.Infrastructure.Data;
using System.Collections.Concurrent;

namespace Orbito.Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

         public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
         {
             if (_transaction != null)
             {
                 throw new InvalidOperationException("Transaction already started");
             }

             _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
         }
        
         public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
         {
             if (_transaction == null)
             {
                 throw new InvalidOperationException("No transaction to commit");
             }
        
             try
             {
                 await _transaction.CommitAsync(cancellationToken);
             }
             catch (Exception)
             {
                 await RollbackTransactionAsync(cancellationToken);
                 throw;
             }
             finally
             {
                 if (_transaction != null)
                 {
                     await _transaction.DisposeAsync();
                     _transaction = null;
                 }
             }
         }
        
         public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
         {
             if (_transaction != null)
             {
                 await _transaction.RollbackAsync(cancellationToken);
                 await _transaction.DisposeAsync();
                 _transaction = null;
             }
         }
        
         public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
         {
             return await _context.SaveChangesAsync(cancellationToken);
         }
        
         public async ValueTask DisposeAsync()
         {
             if (_transaction != null)
             {
                 await _transaction.RollbackAsync();
                 await _transaction.DisposeAsync();
                 _transaction = null;
             }
         }

         public IRepository<T> GetRepository<T>() where T : class
         {
             return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
         }
    }
}
