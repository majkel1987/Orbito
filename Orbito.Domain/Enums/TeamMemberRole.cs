namespace Orbito.Domain.Enums;

/// <summary>
/// Represents the role of a team member within a provider's organization.
/// </summary>
public enum TeamMemberRole
{
    /// <summary>
    /// Owner of the organization with full permissions and team management capabilities.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Administrator with all business operations permissions but cannot manage team.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Regular member with read-only and basic operation permissions.
    /// </summary>
    Member = 3
}
