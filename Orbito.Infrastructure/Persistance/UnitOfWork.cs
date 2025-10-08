using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
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
        private readonly ITenantProvider _tenantProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISecurityLimitService _securityLimitService;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private IProviderRepository? _providers;
        private IClientRepository? _clients;
        private ISubscriptionRepository? _subscriptions;
        private ISubscriptionPlanRepository? _subscriptionPlans;
        private IPaymentRepository? _payments;
        private IPaymentMethodRepository? _paymentMethods;
        private IPaymentRetryRepository? _paymentRetries;
        private IWebhookLogRepository? _webhookLogs;
        private IEmailNotificationRepository? _emailNotifications;

        public UnitOfWork(
            ApplicationDbContext context,
            ITenantContext tenantContext,
            ITenantProvider tenantProvider,
            ILoggerFactory loggerFactory,
            ISecurityLimitService securityLimitService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _securityLimitService = securityLimitService ?? throw new ArgumentNullException(nameof(securityLimitService));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IProviderRepository Providers => _providers ??= new ProviderRepository(_context);
        public IClientRepository Clients => _clients ??= new ClientRepository(_context);
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context, _tenantContext, _loggerFactory.CreateLogger<PaymentRepository>(), _securityLimitService);
        public IPaymentMethodRepository PaymentMethods => _paymentMethods ??= new PaymentMethodRepository(_context, _tenantContext);
        public IPaymentRetryRepository PaymentRetries => _paymentRetries ??= new PaymentRetryRepository(_context, _tenantProvider);
        public IWebhookLogRepository WebhookLogs => _webhookLogs ??= new WebhookLogRepository(_context, _tenantContext);
        public IEmailNotificationRepository EmailNotifications => _emailNotifications ??= new EmailNotificationRepository(_context);

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

         /// <summary>
         /// Begins a database transaction with specified isolation level
         /// Uses EF Core API for proper transaction management
         /// </summary>
         public async Task<Result> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
         {
             try
             {
                 if (_transaction != null)
                 {
                     return Result.Failure("Transaction already started");
                 }

                 // FIXED: Use EF Core API with isolation level parameter instead of raw SQL
                 // This ensures proper transaction management across different database providers
                 _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
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
        
         /// <summary>
         /// Disposes the UnitOfWork and rollbacks any pending transaction
         /// </summary>
         public async ValueTask DisposeAsync()
         {
             if (_transaction != null)
             {
                 // FIXED: Added CancellationToken.None for proper async disposal
                 await _transaction.RollbackAsync(CancellationToken.None);
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
