using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.TeamMembers.Queries.GetPendingInvitations;

/// <summary>
/// Query to get all pending invitations for a provider organization.
/// </summary>
public record GetPendingInvitationsQuery : IRequest<Result<IEnumerable<TeamMemberDto>>>
{
}
