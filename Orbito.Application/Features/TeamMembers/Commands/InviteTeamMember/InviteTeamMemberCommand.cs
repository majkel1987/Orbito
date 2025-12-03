using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;

/// <summary>
/// Command to invite a new team member to a provider organization.
/// </summary>
public record InviteTeamMemberCommand : IRequest<Result<TeamMemberDto>>
{
    /// <summary>
    /// Email address of the person to invite.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Role to assign to the team member.
    /// </summary>
    public required TeamMemberRole Role { get; init; }

    /// <summary>
    /// First name of the person to invite.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name of the person to invite.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Optional message to include in the invitation.
    /// </summary>
    public string? Message { get; init; }
}
