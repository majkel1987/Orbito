using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orbito.Application.Common.Authorization;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using System.Security.Claims;
using Xunit;

namespace Orbito.Tests.Application.Common.Authorization;

[Trait("Category", "Unit")]
public class ProviderTeamAccessHandlerTests
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly ProviderTeamAccessHandler _handler;
    private readonly ProviderTeamAccessRequirement _requirement;
    private readonly TenantId _testTenantId;
    private readonly Guid _testUserId;

    public ProviderTeamAccessHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();

        _testTenantId = TenantId.New();
        _testUserId = Guid.NewGuid();

        // Setup service scope
        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider
            .Setup(x => x.GetService(typeof(ITeamMemberRepository)))
            .Returns(_teamMemberRepositoryMock.Object);

        _serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(scopeServiceProvider.Object);

        _serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _handler = new ProviderTeamAccessHandler(_serviceScopeFactoryMock.Object);
        _requirement = new ProviderTeamAccessRequirement();
    }

    [Fact]
    public async Task HandleRequirement_TeamMemberWithPermission_ShouldSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", _testTenantId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        _teamMemberRepositoryMock
            .Setup(x => x.IsUserTeamMemberAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirement_NonTeamMember_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", _testTenantId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        _teamMemberRepositoryMock
            .Setup(x => x.IsUserTeamMemberAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_WrongProvider_ShouldFail()
    {
        // Arrange
        var differentTenantId = TenantId.New();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", differentTenantId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        _teamMemberRepositoryMock
            .Setup(x => x.IsUserTeamMemberAsync(_testUserId, differentTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_LegacyProviderRole_ShouldSucceed()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", _testTenantId.Value.ToString()),
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
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();

        // Should not check repository for legacy Provider role
        _teamMemberRepositoryMock.Verify(
            x => x.IsUserTeamMemberAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleRequirement_NotAuthenticated_ShouldFail()
    {
        // Arrange
        var identity = new ClaimsIdentity();
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
    public async Task HandleRequirement_MissingUserIdClaim_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("tenant_id", _testTenantId.Value.ToString())
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
    public async Task HandleRequirement_MissingTenantIdClaim_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
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
    public async Task HandleRequirement_InvalidUserIdFormat_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
            new Claim("tenant_id", _testTenantId.Value.ToString())
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
    public async Task HandleRequirement_InvalidTenantIdFormat_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", "invalid-guid")
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

    [Theory]
    [InlineData(TeamMemberRole.Owner)]
    [InlineData(TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Member)]
    public async Task HandleRequirement_AllRoles_ShouldSucceed_WhenIsTeamMember(TeamMemberRole role)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", _testTenantId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        // IsUserTeamMemberAsync checks both existence and active status
        _teamMemberRepositoryMock
            .Setup(x => x.IsUserTeamMemberAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirement_RepositoryException_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim("tenant_id", _testTenantId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(
            new[] { _requirement },
            claimsPrincipal,
            null);

        _teamMemberRepositoryMock
            .Setup(x => x.IsUserTeamMemberAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.HandleAsync(context))
            .Should().ThrowAsync<Exception>();
    }
}
