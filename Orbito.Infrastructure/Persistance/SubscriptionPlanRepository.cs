using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<SubscriptionPlan?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SubscriptionPlans
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool activeOnly = false, bool publicOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionPlans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (activeOnly)
            {
                query = query.Where(p => p.IsActive);
            }

            if (publicOnly)
            {
                query = query.Where(p => p.IsPublic);
            }

            return await query
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync(bool publicOnly = false, int? limit = null, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionPlans
                .Where(p => p.IsActive);

            if (publicOnly)
            {
                query = query.Where(p => p.IsPublic);
            }

            query = query.OrderBy(p => p.SortOrder).ThenBy(p => p.Name);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetPlansBySortOrderAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SubscriptionPlans
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountAsync(string? searchTerm = null, bool activeOnly = false, bool publicOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionPlans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (activeOnly)
            {
                query = query.Where(p => p.IsActive);
            }

            if (publicOnly)
            {
                query = query.Where(p => p.IsPublic);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<SubscriptionPlan> AddAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default)
        {
            _context.SubscriptionPlans.Add(subscriptionPlan);
            await _context.SaveChangesAsync(cancellationToken);
            return subscriptionPlan;
        }

        public async Task UpdateAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default)
        {
            _context.SubscriptionPlans.Update(subscriptionPlan);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(SubscriptionPlan subscriptionPlan, CancellationToken cancellationToken = default)
        {
            _context.SubscriptionPlans.Remove(subscriptionPlan);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CanPlanBeDeletedAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null)
                return false;

            return !plan.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
        }

        public async Task<bool> IsPlanNameUniqueAsync(string name, Guid? excludePlanId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.SubscriptionPlans
                .Where(p => p.Name == name);

            if (excludePlanId.HasValue)
            {
                query = query.Where(p => p.Id != excludePlanId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }
    }
}
