using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;

/// <summary>
/// Command to update a team member's role.
/// </summary>
public record UpdateTeamMemberRoleCommand : IRequest<Result<TeamMemberDto>>
{
    /// <summary>
    /// ID of the team member to update.
    /// </summary>
    public required Guid TeamMemberId { get; init; }

    /// <summary>
    /// New role to assign to the team member.
    /// </summary>
    public required TeamMemberRole NewRole { get; init; }
}
