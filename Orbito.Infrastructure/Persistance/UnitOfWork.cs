using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Orbito.Application.Common.Interfaces;
using Orbito.Infrastructure.Data;
using System.Collections.Concurrent;
using System.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IProviderRepository? _providers;
        private IClientRepository? _clients;
        private ISubscriptionPlanRepository? _subscriptionPlans;
        private IPaymentRepository? _payments;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IProviderRepository Providers => _providers ??= new ProviderRepository(_context);
        public IClientRepository Clients => _clients ??= new ClientRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);

         public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
         {
             if (_transaction != null)
             {
                 throw new InvalidOperationException("Transaction already started");
             }

             _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
         }

         public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
         {
             if (_transaction != null)
             {
                 throw new InvalidOperationException("Transaction already started");
             }

             // Set isolation level via raw SQL if needed
             switch (isolationLevel)
             {
                 case IsolationLevel.ReadCommitted:
                     await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL READ COMMITTED", cancellationToken);
                     break;
                 case IsolationLevel.Serializable:
                     await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE", cancellationToken);
                     break;
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
