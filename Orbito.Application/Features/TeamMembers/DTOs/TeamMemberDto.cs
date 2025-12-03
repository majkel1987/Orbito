using Orbito.Domain.Enums;

namespace Orbito.Application.Features.TeamMembers.DTOs;

/// <summary>
/// Data transfer object for team member information.
/// </summary>
public record TeamMemberDto
{
    /// <summary>
    /// Unique identifier of the team member.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The tenant (provider) this team member belongs to.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// The user ID from the identity system.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The role of this team member within the organization.
    /// </summary>
    public TeamMemberRole Role { get; init; }

    /// <summary>
    /// Email address of the team member.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// First name of the team member.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name of the team member.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Full name of the team member.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Display name of the team member.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Whether the team member is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// When the team member was invited.
    /// </summary>
    public DateTime InvitedAt { get; init; }

    /// <summary>
    /// When the team member was last active.
    /// </summary>
    public DateTime LastActiveAt { get; init; }

    /// <summary>
    /// When the team member accepted the invitation.
    /// </summary>
    public DateTime? AcceptedAt { get; init; }

    /// <summary>
    /// When the team member was removed from the team.
    /// </summary>
    public DateTime? RemovedAt { get; init; }

    /// <summary>
    /// Whether the team member is an owner.
    /// </summary>
    public bool IsOwner { get; init; }

    /// <summary>
    /// Whether the team member is an admin or owner.
    /// </summary>
    public bool IsAdmin { get; init; }

    /// <summary>
    /// Whether the team member has accepted the invitation.
    /// </summary>
    public bool HasAcceptedInvitation { get; init; }
}
