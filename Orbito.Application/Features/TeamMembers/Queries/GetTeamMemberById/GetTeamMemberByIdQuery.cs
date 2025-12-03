using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.TeamMembers.Queries.GetTeamMemberById;

/// <summary>
/// Query to get a specific team member by ID.
/// </summary>
public record GetTeamMemberByIdQuery : IRequest<Result<TeamMemberDto>>
{
    /// <summary>
    /// ID of the team member to retrieve.
    /// </summary>
    public required Guid TeamMemberId { get; init; }
}
