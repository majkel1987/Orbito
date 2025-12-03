using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.Queries.GetTeamMembers;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Features.TeamMembers.Queries.GetTeamMembers;

[Trait("Category", "Unit")]
public class GetTeamMembersQueryHandlerTests : BaseTestFixture
{
    private readonly Mock<ITeamMemberRepository> _teamMemberRepositoryMock;
    private readonly Mock<ILogger<GetTeamMembersQueryHandler>> _loggerMock;
    private readonly GetTeamMembersQueryHandler _handler;

    public GetTeamMembersQueryHandlerTests()
    {
        _teamMemberRepositoryMock = new Mock<ITeamMemberRepository>();
        _loggerMock = new Mock<ILogger<GetTeamMembersQueryHandler>>();

        _handler = new GetTeamMembersQueryHandler(
            _teamMemberRepositoryMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnAllActiveMembers()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeInactive = false
        };

        var teamMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Owner, "owner@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member@example.com")
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        _teamMemberRepositoryMock.Verify(
            x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyTeam_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TeamMember>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 2,
            PageSize = 2,
            IncludeInactive = false
        };

        var teamMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Owner, "owner@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member1@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member2@example.com")
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // Page 2 with size 2 should return items 3 and 4
    }

    [Fact]
    public async Task Handle_FilterByRole_ShouldReturnFilteredResults()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            RoleFilter = "Admin"
        };

        var teamMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Owner, "owner@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin1@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin2@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member@example.com")
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(tm => tm.Role == TeamMemberRole.Admin);
    }

    [Fact]
    public async Task Handle_IncludeInactive_ShouldReturnAllMembers()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeInactive = true
        };

        var activeMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin@example.com")
        };

        var inactiveMember = new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "inactive@example.com");
        inactiveMember.Deactivate();

        var allMembers = new List<TeamMember>(activeMembers) { inactiveMember };

        _teamMemberRepositoryMock
            .Setup(x => x.GetByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _teamMemberRepositoryMock.Verify(
            x => x.GetByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        SetupNoTenantContext();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

        _teamMemberRepositoryMock.Verify(
            x => x.GetActiveByTenantIdAsync(It.IsAny<Orbito.Domain.ValueObjects.TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidRoleFilter_ShouldReturnAllMembers()
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            RoleFilter = "InvalidRole"
        };

        var teamMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member@example.com")
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2); // Invalid filter should not filter anything
    }

    [Theory]
    [InlineData(TeamMemberRole.Owner)]
    [InlineData(TeamMemberRole.Admin)]
    [InlineData(TeamMemberRole.Member)]
    public async Task Handle_FilterBySpecificRole_ShouldReturnOnlyThatRole(TeamMemberRole role)
    {
        // Arrange
        var query = new GetTeamMembersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            RoleFilter = role.ToString()
        };

        var teamMembers = new List<TeamMember>
        {
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Owner, "owner@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Admin, "admin@example.com"),
            new TeamMember(TestTenantId, Guid.NewGuid(), TeamMemberRole.Member, "member@example.com")
        };

        _teamMemberRepositoryMock
            .Setup(x => x.GetActiveByTenantIdAsync(TestTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamMembers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().OnlyContain(tm => tm.Role == role);
    }

}
