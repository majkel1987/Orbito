using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Interfaces;

/// <summary>
/// Repository interface for managing team members.
/// </summary>
public interface ITeamMemberRepository
{
    /// <summary>
    /// Gets a team member by ID.
    /// </summary>
    /// <param name="id">The team member ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The team member if found, null otherwise.</returns>
    Task<TeamMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team member by ID for a specific tenant.
    /// </summary>
    /// <param name="id">The team member ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The team member if found, null otherwise.</returns>
    Task<TeamMember?> GetByIdForTenantAsync(Guid id, TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new team member.
    /// </summary>
    /// <param name="teamMember">The team member to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added team member.</returns>
    Task<TeamMember> AddAsync(TeamMember teamMember, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing team member.
    /// </summary>
    /// <param name="teamMember">The team member to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated team member.</returns>
    Task<TeamMember> UpdateAsync(TeamMember teamMember, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a team member.
    /// </summary>
    /// <param name="teamMember">The team member to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the operation.</returns>
    Task DeleteAsync(TeamMember teamMember, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team member by user ID for a specific tenant.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The team member if found, null otherwise.</returns>
    Task<TeamMember?> GetByUserIdForTenantAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all team members for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of team members.</returns>
    Task<IEnumerable<TeamMember>> GetByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active team members for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active team members.</returns>
    Task<IEnumerable<TeamMember>> GetActiveByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets team members by role for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="role">The role to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of team members with the specified role.</returns>
    Task<IEnumerable<TeamMember>> GetByRoleAsync(TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is a team member of a specific tenant.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is a team member, false otherwise.</returns>
    Task<bool> IsUserTeamMemberAsync(Guid userId, TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role in a tenant.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    Task<bool> HasRoleAsync(Guid userId, TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified roles in a tenant.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="roles">The roles to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has any of the roles, false otherwise.</returns>
    Task<bool> HasAnyRoleAsync(Guid userId, TenantId tenantId, IEnumerable<TeamMemberRole> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of team members for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of team members.</returns>
    Task<int> GetCountByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active team members for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of active team members.</returns>
    Task<int> GetActiveCountByTenantIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of team members by role for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="role">The role to count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of team members with the specified role.</returns>
    Task<int> GetCountByRoleAsync(TenantId tenantId, TeamMemberRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already used in a tenant.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email is already used, false otherwise.</returns>
    Task<bool> IsEmailUsedInTenantAsync(string email, TenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets team members with pagination for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of team members.</returns>
    Task<(IEnumerable<TeamMember> Items, int TotalCount)> GetPagedByTenantIdAsync(
        TenantId tenantId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a team member by invitation token.
    /// </summary>
    /// <param name="token">The invitation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The team member if found, null otherwise.</returns>
    Task<TeamMember?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default);
}
