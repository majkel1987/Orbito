using Orbito.Domain.Entities;

namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Repository for PlatformPlan entity (Orbito's subscription plans for Providers).
/// </summary>
public interface IPlatformPlanRepository
{
    Task<PlatformPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlatformPlan>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PlatformPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PlatformPlan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PlatformPlan> AddAsync(PlatformPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlatformPlan plan, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
