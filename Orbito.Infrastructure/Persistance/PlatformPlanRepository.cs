using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Repository for PlatformPlan entity.
/// PlatformPlans are global (no tenant filtering) - they are Orbito's subscription plans for Providers.
/// </summary>
public class PlatformPlanRepository : IPlatformPlanRepository
{
    private readonly ApplicationDbContext _context;

    public PlatformPlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformPlans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PlatformPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PlatformPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Price.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlatformPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PlatformPlans
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Price.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformPlans
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<PlatformPlan> AddAsync(PlatformPlan plan, CancellationToken cancellationToken = default)
    {
        var entry = await _context.PlatformPlans.AddAsync(plan, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(PlatformPlan plan, CancellationToken cancellationToken = default)
    {
        _context.PlatformPlans.Update(plan);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformPlans.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
