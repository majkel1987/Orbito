using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
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

        public async Task<Provider?> GetBySubdomainSlugAsync(string subdomainSlug, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.SubdomainSlug == subdomainSlug, cancellationToken);
        }

        public async Task<Provider> AddAsync(Provider provider, CancellationToken cancellationToken = default)
        {
            var entry = await _context.Providers.AddAsync(provider, cancellationToken);
            return entry.Entity;
        }

        public async Task<bool> ExistsAsync(string subdomainSlug, CancellationToken cancellationToken = default)
        {
            return await _context.Providers
                .IgnoreQueryFilters()
                .AnyAsync(p => p.SubdomainSlug == subdomainSlug, cancellationToken);
        }
    }
}
