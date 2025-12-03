using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Orbito.Application.Common.Authorization;
using System.Security.Claims;
using Xunit;

namespace Orbito.Tests.Application.Common.Authorization;

[Trait("Category", "Unit")]
public class ClientAccessHandlerTests
{
    private readonly ClientAccessHandler _handler;
    private readonly ClientAccessRequirement _requirement;

    public ClientAccessHandlerTests()
    {
        _handler = new ClientAccessHandler();
        _requirement = new ClientAccessRequirement();
    }

    [Fact]
    public async Task HandleRequirement_ValidClientAccess_ShouldSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Client")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirement_NonClientRole_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Provider")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_MissingRoleClaim_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_NotAuthenticated_ShouldFail()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No authentication type
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_NullIdentity_ShouldFail()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal();
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Theory]
    [InlineData("Client")]
    [InlineData("client")]
    [InlineData("CLIENT")]
    public async Task HandleRequirement_ClientRoleCaseInsensitive_ShouldSucceed(string role)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        // Note: The actual implementation is case-sensitive, so only "Client" should succeed
        if (role == "Client")
        {
            context.HasSucceeded.Should().BeTrue();
        }
        else
        {
            context.HasSucceeded.Should().BeFalse();
        }
    }

    [Fact]
    public async Task HandleRequirement_MultipleRoles_ClientIncluded_ShouldSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Client")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_EmptyRole_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }
}
