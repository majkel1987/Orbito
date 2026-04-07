using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly ApplicationDbContext _context;

        public ProviderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Read operations
        public async Task<Provider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .Include(p => p.User)
                .Include(p => p.Plans)
                .Include(p => p.Clients)
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Provider?> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId, cancellationToken);
        }

        public async Task<Provider?> GetBySubdomainSlugAsync(string subdomainSlug, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .IgnoreQueryFilters()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.SubdomainSlug == subdomainSlug, cancellationToken);
        }

        public async Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Provider>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .Include(p => p.User)
                .OrderBy(p => p.BusinessName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Provider>> GetActiveProvidersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .Where(p => p.IsActive)
                .Include(p => p.User)
                .OrderBy(p => p.BusinessName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Providers.CountAsync(cancellationToken);
        }

        public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Providers.CountAsync(p => p.IsActive, cancellationToken);
        }

        // Create operations
        public async Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default)
        {
            var entry = await _context.Providers.AddAsync(provider, cancellationToken);
            return entry.Entity;
        }

        // Update operations
        public async Task UpdateAsync(Provider provider, CancellationToken cancellationToken = default)
        {
            _context.Providers.Update(provider);
            await Task.CompletedTask;
        }

        // Delete operations
        public async Task DeleteAsync(Provider provider, CancellationToken cancellationToken = default)
        {
            _context.Providers.Remove(provider);
            await Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(Provider provider, CancellationToken cancellationToken = default)
        {
            provider.Deactivate();
            _context.Providers.Update(provider);
            await Task.CompletedTask;
        }

        // Validation operations
        public async Task<bool> ExistsAsync(string subdomainSlug, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .IgnoreQueryFilters()
                .AnyAsync(p => p.SubdomainSlug == subdomainSlug, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> IsSubdomainAvailableAsync(string subdomainSlug, Guid? excludeProviderId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Providers
                .IgnoreQueryFilters()
                .Where(p => p.SubdomainSlug == subdomainSlug);

            if (excludeProviderId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProviderId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        // Platform Admin operations
        public async Task<Guid?> GetPlatformAdminTenantIdAsync(CancellationToken cancellationToken = default)
        {
            // PlatformAdmin's Provider has subdomain "admin"
            var adminProvider = await _context.Providers
                .IgnoreQueryFilters()
                .Where(p => p.SubdomainSlug == "admin")
                .Select(p => new { p.TenantId })
                .FirstOrDefaultAsync(cancellationToken);

            return adminProvider?.TenantId.Value;
        }
    }
}
