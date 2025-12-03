using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Orbito.Application.Common.Authorization;

/// <summary>
/// Authorization handler for ClientAccess policy.
/// Allows access to clients.
/// </summary>
public class ClientAccessHandler : AuthorizationHandler<ClientAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientAccessRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if user has Client role (supports multiple roles)
        if (context.User.IsInRole("Client"))
        {
            context.Succeed(requirement);
        }
        else
        {
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
