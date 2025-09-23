using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<Subscription?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.Payments)
                .Where(s => s.ClientId == clientId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime checkDate, int daysBeforeExpiration = 7, CancellationToken cancellationToken = default)
        {
            var expirationDate = checkDate.AddDays(daysBeforeExpiration);
            
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate <= expirationDate &&
                           s.NextBillingDate > checkDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync(DateTime checkDate, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate <= checkDate)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsByPlanIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.PlanId == planId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> SearchSubscriptionsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => 
                    s.Client.CompanyName!.Contains(searchTerm) ||
                    s.Client.DirectEmail!.Contains(searchTerm) ||
                    s.Client.DirectFirstName!.Contains(searchTerm) ||
                    s.Client.DirectLastName!.Contains(searchTerm) ||
                    s.Plan.Name.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);
            return subscription;
        }

        public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasActiveSubscriptionAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.ClientId == clientId && s.Status == SubscriptionStatus.Active, cancellationToken);
        }

        public async Task<bool> CanClientSubscribeToPlanAsync(Guid clientId, Guid planId, CancellationToken cancellationToken = default)
        {
            // Check if client already has an active subscription to this plan
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.ClientId == clientId && 
                              s.PlanId == planId && 
                              s.Status == SubscriptionStatus.Active, cancellationToken);

            return !hasActiveSubscription;
        }

        public async Task<int> GetActiveSubscriptionsCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken);
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .SumAsync(s => s.CurrentPrice.Amount, cancellationToken);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsForBillingAsync(DateTime billingDate, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Include(s => s.Client)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.NextBillingDate.Date == billingDate.Date)
                .OrderBy(s => s.NextBillingDate)
                .ToListAsync(cancellationToken);
        }
    }
}
