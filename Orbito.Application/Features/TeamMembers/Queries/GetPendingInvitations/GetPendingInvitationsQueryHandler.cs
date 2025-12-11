using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;

namespace Orbito.Application.Features.TeamMembers.Queries.GetPendingInvitations;

/// <summary>
/// Handler for getting all pending invitations for a provider organization.
/// </summary>
public class GetPendingInvitationsQueryHandler : IRequestHandler<GetPendingInvitationsQuery, Result<IEnumerable<TeamMemberDto>>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetPendingInvitationsQueryHandler> _logger;

    public GetPendingInvitationsQueryHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        ILogger<GetPendingInvitationsQueryHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TeamMemberDto>>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to get pending invitations without tenant context");
            return Result.Failure<IEnumerable<TeamMemberDto>>(DomainErrors.Tenant.NoTenantContext);
        }

        var tenantId = _tenantContext.CurrentTenantId;

        // Get all team members for the tenant
        var teamMembers = await _teamMemberRepository.GetByTenantIdAsync(tenantId, cancellationToken);

        // Filter to only pending invitations (not yet accepted)
        var pendingInvitations = teamMembers
            .Where(tm => !tm.HasAcceptedInvitation && tm.InvitationExpiresAt.HasValue && tm.InvitationExpiresAt.Value > DateTime.UtcNow)
            .OrderByDescending(tm => tm.InvitedAt)
            .ToList();

        var dtos = pendingInvitations.Select(tm => tm.ToDto());

        _logger.LogInformation(
            "Retrieved {Count} pending invitations for tenant {TenantId}",
            pendingInvitations.Count,
            tenantId);

        return Result.Success(dtos);
    }
}
