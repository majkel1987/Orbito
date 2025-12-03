using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;

/// <summary>
/// Handler for inviting a new team member to a provider organization.
/// </summary>
public class InviteTeamMemberCommandHandler : IRequestHandler<InviteTeamMemberCommand, Result<TeamMemberDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<InviteTeamMemberCommandHandler> _logger;

    public InviteTeamMemberCommandHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        ILogger<InviteTeamMemberCommandHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TeamMemberDto>> Handle(InviteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to invite team member without tenant context");
            return Result.Failure<TeamMemberDto>(DomainErrors.Tenant.NoTenantContext);
        }

        var tenantId = _tenantContext.CurrentTenantId;

        // Check if email is already used in this tenant
        var isEmailUsed = await _teamMemberRepository.IsEmailUsedInTenantAsync(
            request.Email,
            tenantId,
            cancellationToken);

        if (isEmailUsed)
        {
            _logger.LogWarning("Email {Email} is already used in tenant {TenantId}", request.Email, tenantId);
            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.EmailAlreadyExists);
        }

        // Validate role assignment (only owners can assign owner role)
        if (request.Role == TeamMemberRole.Owner)
        {
            // This should be validated at the authorization level, but double-check here
            _logger.LogWarning("Attempt to assign Owner role without proper authorization");
            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.CannotAssignOwnerRole);
        }

        // Create team member
        var teamMember = new TeamMember(
            tenantId,
            Guid.NewGuid(), // This will be updated when the user accepts the invitation
            request.Role,
            request.Email,
            request.FirstName,
            request.LastName);

        // Add to repository
        await _teamMemberRepository.AddAsync(teamMember, cancellationToken);

        _logger.LogInformation(
            "Team member {Email} invited to tenant {TenantId} with role {Role}",
            request.Email,
            tenantId,
            request.Role);

        return Result.Success(teamMember.ToDto());
    }
}
