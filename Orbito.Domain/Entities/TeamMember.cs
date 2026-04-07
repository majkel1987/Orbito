using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Represents a team member in a provider's organization.
/// </summary>
public class TeamMember : IMustHaveTenant
{
    private TeamMember() { } // EF Core constructor

    public TeamMember(
        TenantId tenantId,
        Guid userId,
        TeamMemberRole role,
        string email,
        string? firstName = null,
        string? lastName = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        UserId = userId;
        Role = role;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        InvitedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
        InvitationToken = GenerateInvitationToken();
        InvitationExpiresAt = DateTime.UtcNow.AddDays(7); // Invitations expire in 7 days
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a unique invitation token.
    /// </summary>
    private static string GenerateInvitationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Trim();
    }

    /// <summary>
    /// Unique identifier for the team member.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant (provider) this team member belongs to.
    /// </summary>
    public TenantId TenantId { get; private set; }

    /// <summary>
    /// The user ID from the identity system.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The role of this team member within the organization.
    /// </summary>
    public TeamMemberRole Role { get; private set; }

    /// <summary>
    /// Email address of the team member.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// First name of the team member.
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// Last name of the team member.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Whether the team member is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// When the team member was invited.
    /// </summary>
    public DateTime InvitedAt { get; private set; }

    /// <summary>
    /// When the team member was last active.
    /// </summary>
    public DateTime LastActiveAt { get; private set; }

    /// <summary>
    /// When the team member accepted the invitation.
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// When the team member was removed from the team.
    /// </summary>
    public DateTime? RemovedAt { get; private set; }

    /// <summary>
    /// Unique invitation token for accepting the invitation.
    /// </summary>
    public string? InvitationToken { get; private set; }

    /// <summary>
    /// When the invitation token expires.
    /// </summary>
    public DateTime? InvitationExpiresAt { get; private set; }

    /// <summary>
    /// Gets the full name of the team member.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets the display name (full name or email if no name).
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : Email;

    /// <summary>
    /// Updates the team member's role.
    /// </summary>
    /// <param name="newRole">The new role to assign.</param>
    public void UpdateRole(TeamMemberRole newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the team member's personal information.
    /// </summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    public void UpdatePersonalInfo(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the team member as accepted the invitation.
    /// </summary>
    /// <param name="userId">The user ID from the identity system.</param>
    /// <returns>Result indicating success or failure</returns>
    public Result AcceptInvitation(Guid userId)
    {
        if (AcceptedAt.HasValue)
            return Result.Failure(DomainErrors.TeamMember.AlreadyAccepted);

        if (!string.IsNullOrEmpty(InvitationToken))
        {
            // Validate invitation token expiration
            if (InvitationExpiresAt.HasValue && InvitationExpiresAt.Value < DateTime.UtcNow)
            {
                return Result.Failure(DomainErrors.TeamMember.InvitationExpired);
            }
        }

        UserId = userId;
        AcceptedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
        InvitationToken = null; // Clear token after acceptance
        InvitationExpiresAt = null;
        return Result.Success();
    }

    /// <summary>
    /// Checks if the invitation token is valid and not expired.
    /// </summary>
    /// <param name="token">The invitation token to validate.</param>
    /// <returns>True if the token is valid and not expired, false otherwise.</returns>
    public bool IsInvitationTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(InvitationToken) || string.IsNullOrWhiteSpace(token))
            return false;

        if (InvitationToken != token)
            return false;

        if (InvitationExpiresAt.HasValue && InvitationExpiresAt.Value < DateTime.UtcNow)
            return false;

        if (AcceptedAt.HasValue)
            return false; // Already accepted

        return true;
    }

    /// <summary>
    /// Deactivates the team member.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        RemovedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the team member.
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            return;

        IsActive = true;
        RemovedAt = null;
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the last active timestamp.
    /// </summary>
    public void UpdateLastActive()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the team member is an owner.
    /// </summary>
    public bool IsOwner => Role == TeamMemberRole.Owner;

    /// <summary>
    /// Checks if the team member is an admin or owner.
    /// </summary>
    public bool IsAdmin => Role == TeamMemberRole.Admin || Role == TeamMemberRole.Owner;

    /// <summary>
    /// Checks if the team member has accepted the invitation.
    /// </summary>
    public bool HasAcceptedInvitation => AcceptedAt.HasValue;

    /// <summary>
    /// When the team member was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the team member was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Sets the updated timestamp (for repository use).
    /// </summary>
    public void SetUpdatedAt(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
