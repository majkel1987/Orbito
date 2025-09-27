using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Infrastructure.Data;

namespace Orbito.Infrastructure.Persistance
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .Include(c => c.User)
                .Include(c => c.Subscriptions)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.DirectEmail == email || c.User!.Email == email, cancellationToken);
        }

        public async Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients
                .Include(c => c.User)
                .AsQueryable();

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
            return await _context.Clients.AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Clients.AnyAsync(c => c.DirectEmail == email || c.User!.Email == email, cancellationToken);
        }

        public async Task<bool> CanClientBeDeletedAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var client = await _context.Clients
                .Include(c => c.Subscriptions)
                .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

            if (client == null) return false;

            return client.CanBeDeleted();
        }

        public async Task<ClientStats> GetClientStatsAsync(CancellationToken cancellationToken = default)
        {
            var totalClients = await _context.Clients.CountAsync(cancellationToken);
            var activeClients = await _context.Clients.CountAsync(c => c.IsActive, cancellationToken);
            var inactiveClients = totalClients - activeClients;
            var clientsWithIdentity = await _context.Clients.CountAsync(c => c.UserId != null, cancellationToken);
            var directClients = totalClients - clientsWithIdentity;

            var clientsWithActiveSubscriptions = await _context.Clients
                .Include(c => c.Subscriptions)
                .CountAsync(c => c.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active), cancellationToken);

            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount.Amount, cancellationToken);

            return new ClientStats
            {
                TotalClients = totalClients,
                ActiveClients = activeClients,
                InactiveClients = inactiveClients,
                ClientsWithIdentity = clientsWithIdentity,
                DirectClients = directClients,
                ClientsWithActiveSubscriptions = clientsWithActiveSubscriptions,
                TotalRevenue = totalRevenue,
                Currency = "USD" // TODO: Get from configuration
            };
        }
    }
}
