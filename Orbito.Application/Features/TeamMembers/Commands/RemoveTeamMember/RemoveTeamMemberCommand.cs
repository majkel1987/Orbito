using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.TeamMembers.Commands.RemoveTeamMember;

/// <summary>
/// Command to remove a team member from a provider organization.
/// </summary>
public record RemoveTeamMemberCommand : IRequest<Result>
{
    /// <summary>
    /// ID of the team member to remove.
    /// </summary>
    public required Guid TeamMemberId { get; init; }

    /// <summary>
    /// Optional reason for removal.
    /// </summary>
    public string? Reason { get; init; }
}
