using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.Commands.RemoveTeamMember;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Features.TeamMembers.Commands.RemoveTeamMember;

[Trait("Category", "Unit")]
public class RemoveTeamMemberCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<ILogger<RemoveTeamMemberCommandHandler>> _loggerMock;
    private readonly RemoveTeamMemberCommandHandler _handler;

    public RemoveTeamMemberCommandHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _loggerMock = new Mock<ILogger<RemoveTeamMemberCommandHandler>>();

        _handler = new RemoveTeamMemberCommandHandler(
            _teamMemberRepositoryMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRemoval_ShouldRemoveTeamMember()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId,
            Reason = "No longer needed"
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

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<TeamMember>(tm => !tm.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RemoveOwner_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId
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
        result.Error.Should().Be(DomainErrors.TeamMember.CannotRemoveOwner);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentMember_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId
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
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId
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
    public async Task Handle_AlreadyInactiveMember_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.AlreadyInactive);

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CrossTenantAccess_ShouldReturnFailure()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId
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

    [Theory]
    [InlineData(TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Member)]
    public async Task Handle_RemoveNonOwner_ShouldSucceed(TeamMemberRole role)
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId,
            Reason = $"Removing {role}"
        };

        var teamMember = new TeamMember(
            TestTenantId,
            Guid.NewGuid(),
            role,
            $"{role.ToString().ToLower()}@example.com");

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

        _teamMemberRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<TeamMember>(tm => !tm.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RemovalWithoutReason_ShouldSucceed()
    {
        // Arrange
        var teamMemberId = Guid.NewGuid();
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = teamMemberId
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
    }
}
