using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.Queries.GetTeamMemberById;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Features.TeamMembers.Queries.GetTeamMemberById;

[Trait("Category", "Unit")]
public class GetTeamMemberByIdQueryHandlerTests : BaseTestFixture
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<ILogger<GetTeamMemberByIdQueryHandler>> _loggerMock;
    private readonly GetTeamMemberByIdQueryHandler _handler;

    public GetTeamMemberByIdQueryHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _loggerMock = new Mock<ILogger<GetTeamMemberByIdQueryHandler>>();

        _handler = new GetTeamMemberByIdQueryHandler(
            _teamMemberRepositoryMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnTeamMember()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            TeamMemberRole.Admin,
            "admin@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("admin@example.com");
        result.Value.Role.Should().Be(TeamMemberRole.Admin);

        _teamMemberRepositoryMock.Verify(
            x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.NotFound);
    }

    [Fact]
    public async Task Handle_CrossTenantAccess_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        // Repository should return null for cross-tenant access due to tenant filtering
        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.NotFound);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        SetupNoTenantContext();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

        _teamMemberRepositoryMock.Verify(
            x => x.GetByIdForTenantAsync(It.IsAny<Guid>(), It.IsAny<Orbito.Domain.ValueObjects.TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TeamMemberRole.Owner)]
    [InlineData(TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Member)]
    public async Task Handle_ValidId_ShouldReturnTeamMemberWithCorrectRole(TeamMemberRole role)
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            role,
            $"{role.ToString().ToLower()}@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(role);
    }

    [Fact]
    public async Task Handle_InactiveMember_ShouldStillReturn()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            TeamMemberRole.Member,
            "inactive@example.com");

        teamMember.Deactivate();

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidId_ShouldIncludeAllMemberDetails()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTeamMemberByIdQuery
        {
            TeamMemberId = teamMemberId
        };

        var teamMember = new TeamMember(
            TestTenantId,
            userId,
            TeamMemberRole.Admin,
            "admin@example.com",
            "John",
            "Doe");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("admin@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Role.Should().Be(TeamMemberRole.Admin);
        result.Value.IsActive.Should().BeTrue();
    }

}
