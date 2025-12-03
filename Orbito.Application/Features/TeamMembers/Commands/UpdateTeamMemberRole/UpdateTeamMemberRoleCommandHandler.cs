using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;

/// <summary>
/// Handler for updating a team member's role.
/// </summary>
public class UpdateTeamMemberRoleCommandHandler : IRequestHandler<UpdateTeamMemberRoleCommand, Result<TeamMemberDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateTeamMemberRoleCommandHandler> _logger;

    public UpdateTeamMemberRoleCommandHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        ILogger<UpdateTeamMemberRoleCommandHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TeamMemberDto>> Handle(UpdateTeamMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to update team member role without tenant context");
            return Result.Failure<TeamMemberDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var tenantId = _tenantContext.CurrentTenantId;

        // Get the team member
        var teamMember = await _teamMemberRepository.GetByIdForTenantAsync(
            request.TeamMemberId,
            tenantId,
            cancellationToken);

        if (teamMember == null)
        {
            _logger.LogWarning("Team member {TeamMemberId} not found in tenant {TenantId}",
                request.TeamMemberId, tenantId);
            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.NotFound);
        }

        // Check if team member is active
        if (!teamMember.IsActive)
        {
            _logger.LogWarning("Cannot update role for inactive team member {TeamMemberId}", request.TeamMemberId);
            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.Inactive);
        }

        // Validate role change
        var validationResult = ValidateRoleChange(teamMember.Role, request.NewRole);
        if (validationResult.IsFailure)
        {
            _logger.LogWarning("Invalid role change from {OldRole} to {NewRole} for team member {TeamMemberId}",
                teamMember.Role, request.NewRole, request.TeamMemberId);
            return Result.Failure<TeamMemberDto>(validationResult.Error);
        }

        // Update the role
        teamMember.UpdateRole(request.NewRole);

        // Update in repository
        await _teamMemberRepository.UpdateAsync(teamMember, cancellationToken);

        _logger.LogInformation(
            "Team member {TeamMemberId} ({Email}) role updated from {OldRole} to {NewRole} in tenant {TenantId}",
            request.TeamMemberId,
            teamMember.Email,
            teamMember.Role,
            request.NewRole,
            tenantId);

        return Result.Success(teamMember.ToDto());
    }

    private static Result ValidateRoleChange(TeamMemberRole currentRole, TeamMemberRole newRole)
    {
        // Cannot change to the same role
        if (currentRole == newRole)
        {
            return Result.Failure(DomainErrors.TeamMember.SameRole);
        }

        // Only owners can assign owner role
        if (newRole == TeamMemberRole.Owner)
        {
            return Result.Failure(DomainErrors.TeamMember.CannotAssignOwnerRole);
        }

        // Cannot demote owner to a lower role
        if (currentRole == TeamMemberRole.Owner && newRole != TeamMemberRole.Owner)
        {
            return Result.Failure(DomainErrors.TeamMember.CannotDemoteOwner);
        }

        return Result.Success();
    }
}
