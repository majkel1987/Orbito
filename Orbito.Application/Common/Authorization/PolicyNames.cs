namespace Orbito.Application.Common.Authorization;

/// <summary>
/// Centralized authorization policy names for the application.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// Policy for provider team access - allows access for team members of a provider organization.
    /// This includes Owner, Admin, and Member roles within the team.
    /// </summary>
    public const string ProviderTeamAccess = "ProviderTeamAccess";

    /// <summary>
    /// Policy for provider owner access - allows access only for owners of a provider organization.
    /// This is the highest level of access within a provider team.
    /// </summary>
    public const string ProviderOwnerOnly = "ProviderOwnerOnly";

    /// <summary>
    /// Policy for client access - allows access for clients of providers.
    /// This is for end-users who subscribe to services.
    /// </summary>
    public const string ClientAccess = "ClientAccess";

    /// <summary>
    /// Policy for platform admin access - allows access for platform administrators.
    /// This is the highest level of access across the entire platform.
    /// </summary>
    public const string PlatformAdminAccess = "PlatformAdminAccess";

    /// <summary>
    /// Policy for admin access - allows access for both platform admins and provider owners.
    /// This combines platform-level and provider-level administrative access.
    /// </summary>
    public const string AdminAccess = "AdminAccess";

    /// <summary>
    /// Policy for provider admin access - allows access for provider owners and admins.
    /// This includes both Owner and Admin roles within provider teams.
    /// </summary>
    public const string ProviderAdminAccess = "ProviderAdminAccess";
}
