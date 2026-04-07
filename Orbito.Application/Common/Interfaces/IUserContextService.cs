namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service for getting user context information from claims
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user's ID from claims
    /// </summary>
    /// <returns>User ID or null if not found</returns>
    Guid? GetCurrentUserId();

    /// <summary>
    /// Gets the current user's client ID from claims and database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Client ID or null if not found</returns>
    Task<Guid?> GetCurrentClientIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current user's email from claims
    /// </summary>
    /// <returns>Email or null if not found</returns>
    string? GetCurrentUserEmail();

    /// <summary>
    /// Gets the current user's display name from claims (typically FirstName LastName or email)
    /// </summary>
    /// <returns>Display name or null if not found</returns>
    string? GetCurrentUserName();

    /// <summary>
    /// Gets the current user's role from claims
    /// </summary>
    /// <returns>Role or null if not found</returns>
    string? GetCurrentUserRole();

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    bool IsAuthenticated();
}
