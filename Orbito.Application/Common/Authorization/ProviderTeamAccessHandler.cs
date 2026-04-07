using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProviderTeamAccessHandler> _logger;

    public ProviderTeamAccessHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProviderTeamAccessHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderTeamAccessRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("ProviderTeamAccess denied - user not authenticated");
            context.Fail();
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("ProviderTeamAccess denied - invalid or missing user ID claim");
            context.Fail();
            return;
        }

        // Get tenant ID from claims
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantIdValue))
        {
            _logger.LogWarning("ProviderTeamAccess denied for user {UserId} - invalid or missing tenant ID claim", userId);
            context.Fail();
            return;
        }

        var tenantId = TenantId.Create(tenantIdValue);

        // Check for backward compatibility with old "Provider" role
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == "Provider")
        {
            _logger.LogDebug("ProviderTeamAccess granted for user {UserId} - legacy Provider role", userId);
            context.Succeed(requirement);
            return;
        }

        // Check if user is a team member
        using var scope = _serviceScopeFactory.CreateScope();
        var teamMemberRepository = scope.ServiceProvider.GetRequiredService<ITeamMemberRepository>();

        var isTeamMember = await teamMemberRepository.IsUserTeamMemberAsync(userId, tenantId);
        if (isTeamMember)
        {
            _logger.LogDebug("ProviderTeamAccess granted for user {UserId} in tenant {TenantId}", userId, tenantIdValue);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("ProviderTeamAccess denied for user {UserId} in tenant {TenantId} - not a team member", userId, tenantIdValue);
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
