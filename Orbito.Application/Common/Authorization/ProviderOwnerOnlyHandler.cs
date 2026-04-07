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
/// Authorization handler for ProviderOwnerOnly policy.
/// Allows access only to the owner of a provider's team.
/// </summary>
public class ProviderOwnerOnlyHandler : AuthorizationHandler<ProviderOwnerOnlyRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProviderOwnerOnlyHandler> _logger;

    public ProviderOwnerOnlyHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProviderOwnerOnlyHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderOwnerOnlyRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("ProviderOwnerOnly denied - user not authenticated");
            context.Fail();
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("ProviderOwnerOnly denied - invalid or missing user ID claim");
            context.Fail();
            return;
        }

        // Get tenant ID from claims
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantIdValue))
        {
            _logger.LogWarning("ProviderOwnerOnly denied for user {UserId} - invalid or missing tenant ID claim", userId);
            context.Fail();
            return;
        }

        var tenantId = TenantId.Create(tenantIdValue);

        // Check for backward compatibility with old "Provider" role
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == "Provider")
        {
            _logger.LogDebug("ProviderOwnerOnly granted for user {UserId} - legacy Provider role", userId);
            context.Succeed(requirement);
            return;
        }

        // Check if user is the team owner
        using var scope = _serviceScopeFactory.CreateScope();
        var teamMemberRepository = scope.ServiceProvider.GetRequiredService<ITeamMemberRepository>();

        var teamMember = await teamMemberRepository.GetByUserIdForTenantAsync(userId, tenantId);
        if (teamMember != null && teamMember.Role == TeamMemberRole.Owner && teamMember.IsActive)
        {
            _logger.LogDebug("ProviderOwnerOnly granted for user {UserId} in tenant {TenantId}", userId, tenantIdValue);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "ProviderOwnerOnly denied for user {UserId} in tenant {TenantId} - not owner or not active (Role: {Role}, IsActive: {IsActive})",
                userId,
                tenantIdValue,
                teamMember?.Role,
                teamMember?.IsActive);
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization requirement for ProviderOwnerOnly policy.
/// </summary>
public class ProviderOwnerOnlyRequirement : IAuthorizationRequirement
{
}
