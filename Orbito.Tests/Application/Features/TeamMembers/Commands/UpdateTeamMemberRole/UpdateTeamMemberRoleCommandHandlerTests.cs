using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;

[Trait("Category", "Unit")]
public class UpdateTeamMemberRoleCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<ILogger<UpdateTeamMemberRoleCommandHandler>> _loggerMock;
    private readonly UpdateTeamMemberRoleCommandHandler _handler;

    public UpdateTeamMemberRoleCommandHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _loggerMock = new Mock<ILogger<UpdateTeamMemberRoleCommandHandler>>();

        _handler = new UpdateTeamMemberRoleCommandHandler(
            _teamMemberRepositoryMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRoleUpdate_ShouldUpdateRole()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            TeamMemberRole.Member,
            "member@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        _teamMemberRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember tm, CancellationToken ct) => tm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Role.Should().Be(TeamMemberRole.Admin);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<TeamMember>(tm => tm.Role == TeamMemberRole.Admin), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OwnerRoleChange_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Owner
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.CannotAssignOwnerRole);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentMember_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.NotFound);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ShouldReturnFailure_WhenNoTenantContext()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
        };

        SetupNoTenantContext();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

        _teamMemberRepositoryMock.Verify(
            x => x.GetByIdForTenantAsync(It.IsAny<Guid>(), It.IsAny<Orbito.Domain.ValueObjects.TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SameRole_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.SameRole);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OwnerDemotion_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            TeamMemberRole.Owner,
            "owner@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.CannotDemoteOwner);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveMember_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.Inactive);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TeamMemberRole.Member, TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Admin, TeamMemberRole.Member)]
    public async Task Handle_RoleChange_ShouldUpdateCorrectly(TeamMemberRole fromRole, TeamMemberRole toRole)
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = toRole
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            fromRole,
            $"member@example.com");

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMember);

        _teamMemberRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember tm, CancellationToken ct) => tm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(toRole);
    }

    [Fact]
    public async Task Handle_CrossTenantAccess_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = teamMemberId,
            NewRole = TeamMemberRole.Admin
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetByIdForTenantAsync(teamMemberId, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.NotFound);
    }
}
