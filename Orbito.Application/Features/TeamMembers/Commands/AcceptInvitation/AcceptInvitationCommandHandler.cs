using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.TeamMembers.DTOs;
using Orbito.Application.Features.TeamMembers.Extensions;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;

namespace Orbito.Application.Features.TeamMembers.Commands.AcceptInvitation;

/// <summary>
/// Handler for accepting a team member invitation.
/// </summary>
public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<TeamMemberDto>>
{
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IUserContextService _userContextService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(
        ITeamMemberRepository teamMemberRepository,
        IUserContextService userContextService,
        IUnitOfWork unitOfWork,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _teamMemberRepository = teamMemberRepository ?? throw new ArgumentNullException(nameof(teamMemberRepository));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TeamMemberDto>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing invitation acceptance for token");

        // Validate token is provided
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            _logger.LogWarning("Attempt to accept invitation with empty token");
            return Result.Failure<TeamMemberDto>(DomainErrors.Validation.Required("Invitation token"));
        }

        // Get current user ID from authentication context
        var currentUserId = _userContextService.GetCurrentUserId();
        if (currentUserId == null)
        {
            _logger.LogWarning("Attempt to accept invitation without authentication");
            return Result.Failure<TeamMemberDto>(DomainErrors.General.Unauthorized);
        }

        // Get team member by invitation token
        var teamMember = await _teamMemberRepository.GetByInvitationTokenAsync(request.Token, cancellationToken);
        if (teamMember == null)
        {
            _logger.LogWarning("Team member invitation not found for token");
            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.NotFound);
        }

        // Validate invitation token
        if (!teamMember.IsInvitationTokenValid(request.Token))
        {
            _logger.LogWarning("Invalid or expired invitation token for team member {TeamMemberId}", teamMember.Id);

            if (teamMember.AcceptedAt.HasValue)
            {
                return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.AlreadyAccepted);
            }

            return Result.Failure<TeamMemberDto>(DomainErrors.TeamMember.InvitationExpired);
        }

        // Verify that the email matches the authenticated user's email (if available)
        var currentUserEmail = _userContextService.GetCurrentUserEmail();
        if (!string.IsNullOrWhiteSpace(currentUserEmail) &&
            !string.Equals(teamMember.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Email mismatch: invitation for {InvitationEmail}, but authenticated user is {AuthenticatedEmail}",
                teamMember.Email, currentUserEmail);
            return Result.Failure<TeamMemberDto>(DomainErrors.General.Unauthorized);
        }

        // Accept the invitation
        // Note: AcceptInvitation may throw InvalidOperationException if already accepted or expired
        // Let it propagate to GlobalExceptionHandler
        teamMember.AcceptInvitation(currentUserId.Value);

        // Update in repository
        await _teamMemberRepository.UpdateAsync(teamMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Team member {TeamMemberId} accepted invitation for email {Email}",
            teamMember.Id,
            teamMember.Email);

        return Result.Success(teamMember.ToDto());
    }
}

