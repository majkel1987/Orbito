namespace Orbito.Domain.Enums;

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Service provider (admin of their tenant)
    /// </summary>
    Provider = 1,

    /// <summary>
    /// Client of a provider
    /// </summary>
    Client = 2,

    /// <summary>
    /// Platform administrator (Orbito platform admin)
    /// </summary>
    PlatformAdmin = 3
}
