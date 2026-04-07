using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Entities;

namespace Orbito.Application.Features.TeamMembers.Extensions;

/// <summary>
/// Extension methods for TeamMember entity.
/// </summary>
public static class TeamMemberExtensions
{
    /// <summary>
    /// Converts a TeamMember entity to a TeamMemberDto.
    /// </summary>
    /// <param name="teamMember">The team member entity.</param>
    /// <returns>The team member DTO.</returns>
    public static TeamMemberDto ToDto(this TeamMember teamMember)
    {
        ArgumentNullException.ThrowIfNull(teamMember);

        return new TeamMemberDto
        {
            Id = teamMember.Id,
            TenantId = teamMember.TenantId.Value,
            UserId = teamMember.UserId,
            Role = teamMember.Role,
            Email = teamMember.Email,
            FirstName = teamMember.FirstName,
            LastName = teamMember.LastName,
            FullName = teamMember.FullName,
            DisplayName = teamMember.DisplayName,
            IsActive = teamMember.IsActive,
            InvitedAt = teamMember.InvitedAt,
            LastActiveAt = teamMember.LastActiveAt,
            AcceptedAt = teamMember.AcceptedAt,
            RemovedAt = teamMember.RemovedAt,
            IsOwner = teamMember.IsOwner,
            IsAdmin = teamMember.IsAdmin,
            HasAcceptedInvitation = teamMember.HasAcceptedInvitation
        };
    }
}
