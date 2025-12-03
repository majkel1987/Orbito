using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.TeamMembers.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a team member invitation using an invitation token.
/// </summary>
public record AcceptInvitationCommand : IRequest<Result<TeamMemberDto>>
{
    /// <summary>
    /// The invitation token received via email or link.
    /// </summary>
    public required string Token { get; init; }
}

