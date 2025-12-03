using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using System.Security.Claims;

namespace Orbito.Application.Common.Authorization;

/// <summary>
/// Authorization handler for ProviderTeamAccess policy.
/// Allows access to any member of a provider's team (Owner, Admin, Member).
/// Includes backward compatibility for old "Provider" role.
/// </summary>
public class ProviderTeamAccessHandler : AuthorizationHandler<ProviderTeamAccessRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProviderTeamAccessHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderTeamAccessRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Fail();
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Fail();
            return;
        }

        // Get tenant ID from claims
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantIdValue))
        {
            context.Fail();
            return;
        }

        var tenantId = TenantId.Create(tenantIdValue);

        // Check for backward compatibility with old "Provider" role
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == "Provider")
        {
            context.Succeed(requirement);
            return;
        }

        // Check if user is a team member
        using var scope = _serviceScopeFactory.CreateScope();
        var teamMemberRepository = scope.ServiceProvider.GetRequiredService<ITeamMemberRepository>();

        var isTeamMember = await teamMemberRepository.IsUserTeamMemberAsync(userId, tenantId);
        if (isTeamMember)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization requirement for ProviderTeamAccess policy.
/// </summary>
public class ProviderTeamAccessRequirement : IAuthorizationRequirement
{
}
