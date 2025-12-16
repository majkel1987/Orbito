using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public ClientRepository(ApplicationDbContext context, ITenantProvider tenantProvider)
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
        /// Applies tenant filtering to query using direct property access
        /// IMPORTANT: Do NOT use EF.Property with value converters - it causes InvalidCastException
        /// </summary>
        private IQueryable<Client> ApplyTenantFilter(IQueryable<Client> query)
        {
            var tenantId = GetCurrentTenantIdForFilter();
            if (tenantId.HasValue)
            {
                // Direct property access - EF Core knows how to translate TenantId value object
                // DO NOT use EF.Property<Guid>(c, "TenantId") - it breaks with value converters
                var tenantIdValue = TenantId.Create(tenantId.Value);
                query = query.Where(c => c.TenantId == tenantIdValue);
            }
            // If no tenant context, return all (admin access)
            return query;
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .Include(c => c.Subscriptions)
                .Include(c => c.Payments)
                .Where(c => c.Id == id)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .Where(c => c.DirectEmail == email || c.User!.Email == email)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetInactiveClientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .Where(c => !c.IsActive)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients.AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<int> GetActiveClientsCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients.Where(c => c.IsActive).AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<int> GetInactiveClientsCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients.Where(c => !c.IsActive).AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CompanyName!.Contains(searchTerm) ||
                    c.DirectEmail!.Contains(searchTerm) ||
                    c.DirectFirstName!.Contains(searchTerm) ||
                    c.DirectLastName!.Contains(searchTerm) ||
                    c.User!.Email!.Contains(searchTerm) ||
                    c.User!.FirstName!.Contains(searchTerm) ||
                    c.User!.LastName!.Contains(searchTerm));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, bool activeOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            query = query.Where(c =>
                c.CompanyName!.Contains(searchTerm) ||
                c.DirectEmail!.Contains(searchTerm) ||
                c.DirectFirstName!.Contains(searchTerm) ||
                c.DirectLastName!.Contains(searchTerm) ||
                c.User!.Email!.Contains(searchTerm) ||
                c.User!.FirstName!.Contains(searchTerm) ||
                c.User!.LastName!.Contains(searchTerm));

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetSearchCountAsync(string searchTerm, bool activeOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients.AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            query = query.Where(c =>
                c.CompanyName!.Contains(searchTerm) ||
                c.DirectEmail!.Contains(searchTerm) ||
                c.DirectFirstName!.Contains(searchTerm) ||
                c.DirectLastName!.Contains(searchTerm) ||
                c.User!.Email!.Contains(searchTerm) ||
                c.User!.FirstName!.Contains(searchTerm) ||
                c.User!.LastName!.Contains(searchTerm));

            return await query.CountAsync(cancellationToken);
        }

        public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync(cancellationToken);
            return client;
        }

        public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Client client, CancellationToken cancellationToken = default)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SoftDeleteAsync(Client client, CancellationToken cancellationToken = default)
        {
            client.Deactivate();
            _context.Clients.Update(client);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Where(c => c.Id == id)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Where(c => c.DirectEmail == email || c.User!.Email == email)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> CanClientBeDeletedAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.Subscriptions)
                .Where(c => c.Id == clientId)
                .AsQueryable();

            // Apply tenant filtering
            query = ApplyTenantFilter(query);

            var client = await query.FirstOrDefaultAsync(cancellationToken);

            if (client == null) return false;

            return client.CanBeDeleted();
        }
    }
}
