using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Orbito.Domain.Enums;
using System.Security.Claims;

namespace Orbito.Application.Common.Authorization;

/// <summary>
/// Authorization handler for ClientAccess policy.
/// Allows access to clients.
/// </summary>
public class ClientAccessHandler : AuthorizationHandler<ClientAccessRequirement>
{
    private readonly ILogger<ClientAccessHandler> _logger;

    public ClientAccessHandler(ILogger<ClientAccessHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientAccessRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("ClientAccess denied - user not authenticated");
            context.Fail();
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = userIdClaim?.Value ?? "unknown";

        // Check if user has Client role (supports multiple roles)
        if (context.User.IsInRole(UserRole.Client.ToString()))
        {
            _logger.LogDebug("ClientAccess granted for user {UserId}", userId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("ClientAccess denied for user {UserId} - not a Client role", userId);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement for ClientAccess policy.
/// </summary>
public class ClientAccessRequirement : IAuthorizationRequirement
{
}
