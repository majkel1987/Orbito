using MediatR;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.TeamMembers.Queries.GetTeamMembers;

/// <summary>
/// Query to get all team members for a provider organization.
/// </summary>
public record GetTeamMembersQuery : IRequest<Result<IEnumerable<TeamMemberDto>>>
{
    /// <summary>
    /// Whether to include inactive team members.
    /// </summary>
    public bool IncludeInactive { get; init; } = false;

    /// <summary>
    /// Optional role filter.
    /// </summary>
    public string? RoleFilter { get; init; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size for pagination.
    /// </summary>
    public int PageSize { get; init; } = 50;
}
