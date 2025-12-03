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

namespace Orbito.Application.Features.TeamMembers.Queries.GetTeamMembers;

/// <summary>
/// Handler for getting all team members for a provider organization.
/// </summary>
public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, Result<IEnumerable<TeamMemberDto>>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTeamMembersQueryHandler> _logger;

    public GetTeamMembersQueryHandler(
        ITeamMemberRepository teamMemberRepository,
        ITenantContext tenantContext,
        ILogger<GetTeamMembersQueryHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TeamMemberDto>>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant context
        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempt to get team members without tenant context");
            return Result.Failure<IEnumerable<TeamMemberDto>>(DomainErrors.Tenant.NoTenantContext);
        }

        var tenantId = _tenantContext.CurrentTenantId;

        // Get team members based on filters
        IEnumerable<TeamMember> teamMembers;

        if (request.IncludeInactive)
        {
            teamMembers = await _teamMemberRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        }
        else
        {
            teamMembers = await _teamMemberRepository.GetActiveByTenantIdAsync(tenantId, cancellationToken);
        }

        // Apply role filter if specified
        if (!string.IsNullOrEmpty(request.RoleFilter) &&
            Enum.TryParse<TeamMemberRole>(request.RoleFilter, true, out var roleFilter))
        {
            teamMembers = teamMembers.Where(tm => tm.Role == roleFilter);
        }

        // Apply pagination
        var pagedMembers = teamMembers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = pagedMembers.Select(tm => tm.ToDto());

        _logger.LogInformation(
            "Retrieved {Count} team members for tenant {TenantId} (Page {PageNumber}, Size {PageSize})",
            pagedMembers.Count,
            tenantId,
            request.PageNumber,
            request.PageSize);

        return Result.Success(dtos);
    }
}
