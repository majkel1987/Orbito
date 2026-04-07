using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;

namespace Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;

/// <summary>
/// Handler for inviting a new team member to a provider organization.
/// Creates the team member record and sends an invitation email.
/// </summary>
public class InviteTeamMemberCommandHandler : IRequestHandler<InviteTeamMemberCommand, Result<TeamMemberDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUserContextService _userContextService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InviteTeamMemberCommandHandler> _logger;

    public InviteTeamMemberCommandHandler(
        ITeamMemberRepository teamMemberRepository,
        IProviderRepository providerRepository,
        ITenantContext tenantContext,
        IUserContextService userContextService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<InviteTeamMemberCommandHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository;
        _providerRepository = providerRepository;
        _tenantContext = tenantContext;
        _userContextService = userContextService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
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

        // Add to repository and save FIRST (DB save before email - email failure should not block invitation)
        await _teamMemberRepository.AddAsync(teamMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Team member {Email} invited to tenant {TenantId} with role {Role}",
            request.Email,
            tenantId,
            request.Role);

        // Send invitation email (after DB save - non-blocking)
        await SendInvitationEmailAsync(teamMember, request.Message, cancellationToken);

        return Result.Success(teamMember.ToDto());
    }

    private async Task SendInvitationEmailAsync(
        TeamMember teamMember,
        string? personalMessage,
        CancellationToken cancellationToken)
    {
        // Get provider name for the email
        var provider = await _providerRepository.GetByIdAsync(teamMember.TenantId, cancellationToken);
        var providerName = provider?.BusinessName ?? "Your organization";

        // Get inviter name
        var inviterName = _userContextService.GetCurrentUserName() ?? "A team administrator";

        // Build invitation link
        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var invitationLink = $"{frontendBaseUrl}/accept-invitation?token={teamMember.InvitationToken}";

        // Build invitee name
        var inviteeName = !string.IsNullOrWhiteSpace(teamMember.FirstName)
            ? $"{teamMember.FirstName} {teamMember.LastName}".Trim()
            : teamMember.Email;

        var emailResult = await _emailService.SendTeamMemberInvitationAsync(
            teamMember.Email,
            inviteeName,
            providerName,
            inviterName,
            teamMember.Role.ToString(),
            invitationLink,
            personalMessage,
            cancellationToken);

        if (emailResult.IsFailure)
        {
            // Log error but don't fail the operation - invitation is saved in DB
            _logger.LogError(
                "Failed to send invitation email to {Email} for tenant {TenantId}: {Error}",
                teamMember.Email,
                teamMember.TenantId,
                emailResult.Error);
        }
        else
        {
            _logger.LogInformation(
                "Sent invitation email to {Email} for tenant {TenantId}",
                teamMember.Email,
                teamMember.TenantId);
        }
    }
}
