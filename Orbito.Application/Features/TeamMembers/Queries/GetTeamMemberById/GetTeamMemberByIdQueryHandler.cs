using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.TeamMembers.Queries.GetTeamMemberById;

/// <summary>
/// Handler for getting a specific team member by ID.
/// </summary>
public class GetTeamMemberByIdQueryHandler : IRequestHandler<GetTeamMemberByIdQuery, Result<TeamMemberDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTeamMemberByIdQueryHandler> _logger;

    public GetTeamMemberByIdQueryHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        ILogger<GetTeamMemberByIdQueryHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TeamMemberDto>> Handle(GetTeamMemberByIdQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to get team member without tenant context");
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

        _logger.LogInformation(
            "Retrieved team member {TeamMemberId} ({Email}) for tenant {TenantId}",
            request.TeamMemberId,
            teamMember.Email,
            tenantId);

        return Result.Success(teamMember.ToDto());
    }
}
