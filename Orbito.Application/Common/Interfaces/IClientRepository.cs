using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces
{
    public interface IClientRepository
    {
        // Read operations
        Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetActiveClientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetInactiveClientsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> GetActiveClientsCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<int> GetInactiveClientsCountAsync(string? searchTerm = null, CancellationToken cancellationToken = default);

        // Search operations
        Task<IEnumerable<Client>> SearchClientsAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, bool activeOnly = false, CancellationToken cancellationToken = default);
        Task<int> GetSearchCountAsync(string searchTerm, bool activeOnly = false, CancellationToken cancellationToken = default);

        // Create operations
        Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);

        // Update operations
        Task UpdateAsync(Client client, CancellationToken cancellationToken = default);

        // Delete operations
        Task DeleteAsync(Client client, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(Client client, CancellationToken cancellationToken = default);

        // Validation operations
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> CanClientBeDeletedAsync(Guid clientId, CancellationToken cancellationToken = default);
    }
}
