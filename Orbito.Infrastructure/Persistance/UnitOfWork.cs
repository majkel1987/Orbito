using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Infrastructure.Data;
using Orbito.Infrastructure.Persistence;
using System.Collections.Concurrent;
using System.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantContext _tenantContext;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IProviderRepository? _providers;
        private IClientRepository? _clients;
        private ISubscriptionRepository? _subscriptions;
        private ISubscriptionPlanRepository? _subscriptionPlans;
        private IPaymentRepository? _payments;
        private IPaymentMethodRepository? _paymentMethods;
        private IWebhookLogRepository? _webhookLogs;

        public UnitOfWork(ApplicationDbContext context, ITenantContext tenantContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IProviderRepository Providers => _providers ??= new ProviderRepository(_context);
        public IClientRepository Clients => _clients ??= new ClientRepository(_context);
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context, _tenantContext);
        public IPaymentMethodRepository PaymentMethods => _paymentMethods ??= new PaymentMethodRepository(_context, _tenantContext);
        public IWebhookLogRepository WebhookLogs => _webhookLogs ??= new WebhookLogRepository(_context, _tenantContext);

        public bool HasActiveTransaction => _transaction != null;

         public async Task<Result> BeginTransactionAsync(CancellationToken cancellationToken = default)
         {
             try
             {
                 if (_transaction != null)
                 {
                     return Result.Failure("Transaction already started");
                 }

                 _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                 return Result.Success();
             }
             catch (Exception ex)
             {
                 return Result.Failure(ex);
             }
         }

         public async Task<Result> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
         {
             try
             {
                 if (_transaction != null)
                 {
                     return Result.Failure("Transaction already started");
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
                 return Result.Success();
             }
             catch (Exception ex)
             {
                 return Result.Failure(ex);
             }
         }
        
         public async Task<Result> CommitAsync(CancellationToken cancellationToken = default)
         {
             try
             {
                 if (_transaction == null)
                 {
                     return Result.Failure("No transaction to commit");
                 }
        
                 await _transaction.CommitAsync(cancellationToken);
                 await _transaction.DisposeAsync();
                 _transaction = null;
                 return Result.Success();
             }
             catch (Exception ex)
             {
                 await RollbackAsync(cancellationToken);
                 return Result.Failure(ex);
             }
         }

         public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
         {
             var result = await CommitAsync(cancellationToken);
             if (!result.IsSuccess)
             {
                 throw new InvalidOperationException(result.ErrorMessage);
             }
         }
        
         public async Task<Result> RollbackAsync(CancellationToken cancellationToken = default)
         {
             try
             {
                 if (_transaction != null)
                 {
                     await _transaction.RollbackAsync(cancellationToken);
                     await _transaction.DisposeAsync();
                     _transaction = null;
                 }
                 return Result.Success();
             }
             catch (Exception ex)
             {
                 return Result.Failure(ex);
             }
         }

         public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
         {
             var result = await RollbackAsync(cancellationToken);
             if (!result.IsSuccess)
             {
                 throw new InvalidOperationException(result.ErrorMessage);
             }
         }
        
         public async Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
         {
             try
             {
                 var result = await _context.SaveChangesAsync(cancellationToken);
                 return Result<int>.Success(result);
             }
             catch (Exception ex)
             {
                 return Result<int>.Failure(ex);
             }
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
