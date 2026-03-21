using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using System.Security.Claims;

namespace Orbito.Application.Common.Authorization;

/// <summary>
/// Authorization handler for ActiveProviderSubscription policy.
/// Blocks access for providers with expired trial or subscription.
/// PlatformAdmin is exempt from this restriction.
/// Providers must have an active trial or paid subscription to access protected resources.
/// </summary>
public class ActiveProviderSubscriptionHandler : AuthorizationHandler<ActiveProviderSubscriptionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ActiveProviderSubscriptionHandler> _logger;

    public ActiveProviderSubscriptionHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ActiveProviderSubscriptionHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveProviderSubscriptionRequirement requirement)
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

        // PlatformAdmin is exempt from trial/subscription restrictions
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value == UserRole.PlatformAdmin.ToString())
        {
            _logger.LogDebug("PlatformAdmin {UserId} bypasses subscription check", userId);
            context.Succeed(requirement);
            return;
        }

        // Client role is exempt - they don't have provider subscriptions
        if (roleClaim?.Value == UserRole.Client.ToString())
        {
            _logger.LogDebug("Client {UserId} bypasses subscription check", userId);
            context.Succeed(requirement);
            return;
        }

        // Get tenant ID from claims
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantIdValue))
        {
            _logger.LogWarning("User {UserId} has no tenant_id claim", userId);
            context.Fail();
            return;
        }

        // Check provider subscription status
        using var scope = _serviceScopeFactory.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<IProviderSubscriptionRepository>();

        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantIdValue);

        if (subscription == null)
        {
            _logger.LogWarning(
                "No provider subscription found for tenant {TenantId}. Blocking access.",
                tenantIdValue);
            context.Fail();
            return;
        }

        // Check if subscription is active (trial or paid)
        if (subscription.IsTrialActive || subscription.Status == ProviderSubscriptionStatus.Active)
        {
            _logger.LogDebug(
                "Provider subscription {SubscriptionId} is active (Status: {Status}, IsTrialActive: {IsTrialActive})",
                subscription.Id,
                subscription.Status,
                subscription.IsTrialActive);
            context.Succeed(requirement);
            return;
        }

        // Subscription is expired or cancelled - block access
        _logger.LogInformation(
            "Provider subscription {SubscriptionId} is {Status}. Blocking access for user {UserId}.",
            subscription.Id,
            subscription.Status,
            userId);
        context.Fail();
    }
}

/// <summary>
/// Authorization requirement for ActiveProviderSubscription policy.
/// Requires provider to have an active trial or paid subscription.
/// </summary>
public class ActiveProviderSubscriptionRequirement : IAuthorizationRequirement
{
}
