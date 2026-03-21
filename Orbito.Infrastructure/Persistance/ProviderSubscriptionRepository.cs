using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance;

/// <summary>
/// Repository for ProviderSubscription entity.
/// ProviderSubscriptions are global (no tenant filtering) - they track Provider's subscription to Orbito platform.
/// </summary>
public class ProviderSubscriptionRepository : IProviderSubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderSubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderSubscriptions
            .Include(ps => ps.Provider)
            .Include(ps => ps.PlatformPlan)
            .FirstOrDefaultAsync(ps => ps.Id == id, cancellationToken);
    }

    public async Task<ProviderSubscription?> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderSubscriptions
            .Include(ps => ps.Provider)
            .Include(ps => ps.PlatformPlan)
            .FirstOrDefaultAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }

    public async Task<IEnumerable<ProviderSubscription>> GetByStatusAsync(ProviderSubscriptionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderSubscriptions
            .Include(ps => ps.Provider)
            .Include(ps => ps.PlatformPlan)
            .Where(ps => ps.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderSubscription>> GetExpiringTrialsAsync(int daysUntilExpiration, CancellationToken cancellationToken = default)
    {
        var expirationThreshold = DateTime.UtcNow.AddDays(daysUntilExpiration);

        return await _context.ProviderSubscriptions
            .Include(ps => ps.Provider)
                .ThenInclude(p => p.User)
            .Include(ps => ps.PlatformPlan)
            .Where(ps => ps.Status == ProviderSubscriptionStatus.Trial
                      && ps.TrialEndDate <= expirationThreshold
                      && ps.TrialEndDate > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderSubscription>> GetExpiredTrialsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderSubscriptions
            .Include(ps => ps.Provider)
                .ThenInclude(p => p.User)
            .Include(ps => ps.PlatformPlan)
            .Where(ps => ps.Status == ProviderSubscriptionStatus.Trial
                      && ps.TrialEndDate < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProviderSubscription> AddAsync(ProviderSubscription subscription, CancellationToken cancellationToken = default)
    {
        var entry = await _context.ProviderSubscriptions.AddAsync(subscription, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(ProviderSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.ProviderSubscriptions.Update(subscription);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsForProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderSubscriptions
            .AnyAsync(ps => ps.ProviderId == providerId, cancellationToken);
    }
}
