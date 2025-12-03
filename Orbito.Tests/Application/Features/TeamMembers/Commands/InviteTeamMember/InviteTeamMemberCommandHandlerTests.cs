using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Features.TeamMembers.Commands.InviteTeamMember;

[Trait("Category", "Unit")]
public class InviteTeamMemberCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<ILogger<InviteTeamMemberCommandHandler>> _loggerMock;
    private readonly InviteTeamMemberCommandHandler _handler;

    public InviteTeamMemberCommandHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _loggerMock = new Mock<ILogger<InviteTeamMemberCommandHandler>>();

        _handler = new InviteTeamMemberCommandHandler(
            _teamMemberRepositoryMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidInvitation_ShouldCreateTeamMember()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "newmember@example.com",
            Role = TeamMemberRole.Member,
            FirstName = "John",
            LastName = "Doe",
            Message = "Welcome to our team!"
        };

        _teamMemberRepositoryMock
            .Setup(x => x.IsEmailUsedInTenantAsync(command.Email, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _teamMemberRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember tm, CancellationToken ct) => tm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(command.Email);
        result.Value.Role.Should().Be(command.Role);
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);

        _teamMemberRepositoryMock.Verify(
            x => x.AddAsync(It.Is<TeamMember>(tm =>
                tm.Email == command.Email &&
                tm.Role == command.Role &&
                tm.TenantId == TestTenantId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "existing@example.com",
            Role = TeamMemberRole.Member
        };

        _teamMemberRepositoryMock
            .Setup(x => x.IsEmailUsedInTenantAsync(command.Email, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.EmailAlreadyExists);

        _teamMemberRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "duplicate@example.com",
            Role = TeamMemberRole.Admin
        };

        _teamMemberRepositoryMock
            .Setup(x => x.IsEmailUsedInTenantAsync(command.Email, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.EmailAlreadyExists);
    }

    [Fact]
    public async Task Handle_InvalidRole_ShouldReturnFailure_WhenTryingToAssignOwnerRole()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "newowner@example.com",
            Role = TeamMemberRole.Owner
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.TeamMember.CannotAssignOwnerRole);

        _teamMemberRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotProvider_ShouldReturnFailure_WhenNoTenantContext()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "member@example.com",
            Role = TeamMemberRole.Member
        };

        SetupNoTenantContext();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

        _teamMemberRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Member)]
    public async Task Handle_VariousRoles_ShouldAssignCorrectly(TeamMemberRole role)
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = $"{role.ToString().ToLower()}@example.com",
            Role = role
        };

        _teamMemberRepositoryMock
            .Setup(x => x.IsEmailUsedInTenantAsync(command.Email, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _teamMemberRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeamMember tm, CancellationToken ct) => tm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(role);
    }

    [Fact]
    public async Task Handle_ValidInvitation_ShouldCreateTeamMemberWithInvitationToken()
    {
        // Arrange
        var command = new InviteTeamMemberCommand
        {
            Email = "invited@example.com",
            Role = TeamMemberRole.Member
        };

        TeamMember? capturedTeamMember = null;

        _teamMemberRepositoryMock
            .Setup(x => x.IsEmailUsedInTenantAsync(command.Email, TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _teamMemberRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<TeamMember>(), It.IsAny<CancellationToken>()))
            .Callback<TeamMember, CancellationToken>((tm, ct) => capturedTeamMember = tm)
            .ReturnsAsync((TeamMember tm, CancellationToken ct) => tm);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        capturedTeamMember.Should().NotBeNull();
        capturedTeamMember!.InvitationToken.Should().NotBeNullOrWhiteSpace();
        capturedTeamMember.InvitationExpiresAt.Should().NotBeNull();
        capturedTeamMember.InvitationExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

}
