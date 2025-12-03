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
public class ProviderOwnerOnlyHandlerTests
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly ProviderOwnerOnlyHandler _handler;
    private readonly ProviderOwnerOnlyRequirement _requirement;
    private readonly TenantId _testTenantId;
    private readonly Guid _testUserId;

    public ProviderOwnerOnlyHandlerTests()
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

        _handler = new ProviderOwnerOnlyHandler(_serviceScopeFactoryMock.Object);
        _requirement = new ProviderOwnerOnlyRequirement();
    }

    [Fact]
    public async Task HandleRequirement_ProviderOwner_ShouldSucceed()
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

        var teamMember = new TeamMember(_testTenantId, _testUserId, TeamMemberRole.Owner, "owner@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByUserIdForTenantAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirement_ProviderAdmin_ShouldFail()
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

        var teamMember = new TeamMember(_testTenantId, _testUserId, TeamMemberRole.Admin, "admin@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByUserIdForTenantAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_ProviderMember_ShouldFail()
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

        var teamMember = new TeamMember(_testTenantId, _testUserId, TeamMemberRole.Member, "member@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByUserIdForTenantAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

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
            .Setup(x => x.GetByUserIdForTenantAsync(_testUserId, differentTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);

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

        // Should not even check repository for legacy Provider role
        _teamMemberRepositoryMock.Verify(
            x => x.GetByUserIdForTenantAsync(It.IsAny<Guid>(), It.IsAny<TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleRequirement_MissingClaims_ShouldFail()
    {
        // Arrange
        var claims = new List<Claim>();
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
    public async Task HandleRequirement_InactiveOwner_ShouldFail()
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

        var teamMember = new TeamMember(_testTenantId, _testUserId, TeamMemberRole.Owner, "owner@example.com");
        teamMember.Deactivate();

        _teamMemberRepositoryMock
            .Setup(x => x.GetByUserIdForTenantAsync(_testUserId, _testTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

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
}
