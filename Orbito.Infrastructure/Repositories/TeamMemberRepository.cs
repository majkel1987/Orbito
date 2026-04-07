using Microsoft.EntityFrameworkCore;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.Data;
using Orbito.Infrastructure.Persistance;

namespace Orbito.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing team members.
/// </summary>
public class TeamMemberRepository : ITeamMemberRepository
{
    protected readonly ApplicationDbContext _context;

    public TeamMemberRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TeamMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.Id == id, cancellationToken);
    }

    public async Task<TeamMember> AddAsync(TeamMember teamMember, CancellationToken cancellationToken = default)
    {
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync(cancellationToken);
        return teamMember;
    }

    public async Task<TeamMember> UpdateAsync(TeamMember teamMember, CancellationToken cancellationToken = default)
    {
        teamMember.SetUpdatedAt(DateTime.UtcNow);
        _context.TeamMembers.Update(teamMember);
        await _context.SaveChangesAsync(cancellationToken);
        return teamMember;
    }

    public async Task DeleteAsync(TeamMember teamMember, CancellationToken cancellationToken = default)
    {
        _context.TeamMembers.Remove(teamMember);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TeamMember?> GetByIdForTenantAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Where(tm => tm.Id == id && tm.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TeamMember?> GetByUserIdForTenantAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Where(tm => tm.UserId == userId && tm.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeamMember>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Where(tm => tm.TenantId == tenantId)
            .OrderBy(tm => tm.FirstName)
            .ThenBy(tm => tm.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeamMember>> GetActiveByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Where(tm => tm.TenantId == tenantId && tm.IsActive)
            .OrderBy(tm => tm.FirstName)
            .ThenBy(tm => tm.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeamMember>> GetByRoleAsync(TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .Where(tm => tm.TenantId == tenantId && tm.Role == role && tm.IsActive)
            .OrderBy(tm => tm.FirstName)
            .ThenBy(tm => tm.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUserTeamMemberAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.UserId == userId && tm.TenantId == tenantId && tm.IsActive, cancellationToken);
    }

    public async Task<bool> HasRoleAsync(Guid userId, TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.UserId == userId && tm.TenantId == tenantId && tm.Role == role && tm.IsActive, cancellationToken);
    }

    public async Task<bool> HasAnyRoleAsync(Guid userId, TenantId tenantId, IEnumerable<TeamMemberRole> roles, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.UserId == userId && tm.TenantId == tenantId && roles.Contains(tm.Role) && tm.IsActive, cancellationToken);
    }

    public async Task<int> GetCountByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .CountAsync(tm => tm.TenantId == tenantId, cancellationToken);
    }

    public async Task<int> GetActiveCountByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .CountAsync(tm => tm.TenantId == tenantId && tm.IsActive, cancellationToken);
    }

    public async Task<int> GetCountByRoleAsync(TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .CountAsync(tm => tm.TenantId == tenantId && tm.Role == role && tm.IsActive, cancellationToken);
    }

    public async Task<bool> IsEmailUsedInTenantAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.Email == email && tm.TenantId == tenantId, cancellationToken);
    }

    public async Task<(IEnumerable<TeamMember> Items, int TotalCount)> GetPagedByTenantIdAsync(
        TenantId tenantId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.TeamMembers
            .Where(tm => tm.TenantId == tenantId)
            .OrderBy(tm => tm.FirstName)
            .ThenBy(tm => tm.LastName);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<TeamMember?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.InvitationToken == token, cancellationToken);
    }
}
