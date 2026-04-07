using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Repository for SubscriptionPlan entity with tenant isolation.
/// All queries are automatically filtered by current tenant context.
/// </summary>
public class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public SubscriptionPlanRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Gets current tenant ID for filtering, or returns null if admin access is needed
    /// </summary>
    private Guid? GetCurrentTenantIdForFilter()
    {
        try
        {
            var tenantId = _tenantProvider.GetCurrentTenantIdAsGuid();
            return tenantId == Guid.Empty ? null : tenantId;
        }
        catch
        {
            return null; // Admin access if tenant context unavailable
        }
    }

    /// <summary>
    /// Applies tenant filtering to query using direct property access.
    /// IMPORTANT: Do NOT use EF.Property with value converters - it causes InvalidCastException
    /// </summary>
    private IQueryable<SubscriptionPlan> ApplyTenantFilter(IQueryable<SubscriptionPlan> query)
    {
        var tenantId = GetCurrentTenantIdForFilter();
        if (tenantId.HasValue)
        {
            // Direct property access - EF Core knows how to translate TenantId value object
            // DO NOT use EF.Property<Guid>(p, "TenantId") - it breaks with value converters
            var tenantIdValue = TenantId.Create(tenantId.Value);
            query = query.Where(p => p.TenantId == tenantIdValue);
        }
        // If no tenant context, return all (admin access)
        return query;
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans
            .Where(p => p.Id == id)
            .AsQueryable();

        query = ApplyTenantFilter(query);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans
            .Include(p => p.Subscriptions)
            .Where(p => p.Id == id)
            .AsQueryable();

        query = ApplyTenantFilter(query);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool activeOnly = false,
        bool publicOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans
            .Include(p => p.Subscriptions)
            .AsQueryable();

        // Apply tenant filtering first
        query = ApplyTenantFilter(query);

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

    public async Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync(
        bool publicOnly = false,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans
            .Include(p => p.Subscriptions)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Apply tenant filtering
        query = ApplyTenantFilter(query);

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
        var query = _context.SubscriptionPlans.AsQueryable();

        // Apply tenant filtering
        query = ApplyTenantFilter(query);

        return await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        string? searchTerm = null,
        bool activeOnly = false,
        bool publicOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans.AsQueryable();

        // Apply tenant filtering first
        query = ApplyTenantFilter(query);

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
        var query = _context.SubscriptionPlans
            .Include(p => p.Subscriptions)
            .Where(p => p.Id == planId)
            .AsQueryable();

        // Apply tenant filtering
        query = ApplyTenantFilter(query);

        var plan = await query.FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
            return false;

        // Check if there are any active subscriptions
        return !plan.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
    }

    public async Task<bool> IsPlanNameUniqueAsync(string name, Guid? excludePlanId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans
            .Where(p => p.Name == name)
            .AsQueryable();

        // Apply tenant filtering - name uniqueness is per-tenant
        query = ApplyTenantFilter(query);

        if (excludePlanId.HasValue)
        {
            query = query.Where(p => p.Id != excludePlanId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
