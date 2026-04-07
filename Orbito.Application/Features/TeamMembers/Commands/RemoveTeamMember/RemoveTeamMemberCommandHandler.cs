using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.TeamMembers.Commands.RemoveTeamMember;

/// <summary>
/// Handler for removing a team member from a provider organization.
/// </summary>
public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Result>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveTeamMemberCommandHandler> _logger;

    public RemoveTeamMemberCommandHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<RemoveTeamMemberCommandHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to remove team member without tenant context");
            return Result.Failure(DomainErrors.Tenant.NoTenantContext);
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
            return Result.Failure(DomainErrors.TeamMember.NotFound);
        }

        // Prevent owner from removing themselves
        if (teamMember.IsOwner)
        {
            _logger.LogWarning("Attempt to remove owner {TeamMemberId} from tenant {TenantId}",
                request.TeamMemberId, tenantId);
            return Result.Failure(DomainErrors.TeamMember.CannotRemoveOwner);
        }

        // Check if team member is already inactive
        if (!teamMember.IsActive)
        {
            _logger.LogWarning("Team member {TeamMemberId} is already inactive", request.TeamMemberId);
            return Result.Failure(DomainErrors.TeamMember.AlreadyInactive);
        }

        // Deactivate the team member
        teamMember.Deactivate();

        // Update in repository and save
        await _teamMemberRepository.UpdateAsync(teamMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Team member {TeamMemberId} ({Email}) removed from tenant {TenantId}. Reason: {Reason}",
            request.TeamMemberId,
            teamMember.Email,
            tenantId,
            request.Reason ?? "No reason provided");

        return Result.Success();
    }
}
