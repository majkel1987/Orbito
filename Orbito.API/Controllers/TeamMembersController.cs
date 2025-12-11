using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Orbito.API.Controllers;
using Orbito.Application.Common.Authorization;
using Microsoft.Extensions.Logging;
using Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;
using Orbito.Application.Features.TeamMembers.Commands.RemoveTeamMember;
using Orbito.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;
using Orbito.Application.Features.TeamMembers.Commands.AcceptInvitation;
using Orbito.Application.Features.TeamMembers.Queries.GetTeamMemberById;
using Orbito.Application.Features.TeamMembers.Queries.GetTeamMembers;
using Orbito.Application.Features.TeamMembers.Queries.GetPendingInvitations;
using Orbito.Domain.Enums;
using System.Security.Claims;

namespace Orbito.API.Controllers;

/// <summary>
/// Controller for managing team members within a provider organization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamMembersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeamMembersController> _logger;

        public TeamMembersController(IMediator mediator, ILogger<TeamMembersController> logger) : base(mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

    /// <summary>
    /// Gets all team members for the current provider organization.
    /// </summary>
    /// <returns>List of team members.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetTeamMembers()
    {
        var query = new GetTeamMembersQuery();
        var result = await _mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets all pending invitations for the current provider organization.
    /// </summary>
    /// <returns>List of pending invitations.</returns>
    [HttpGet("invitations")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetPendingInvitations()
    {
        var query = new GetPendingInvitationsQuery();
        var result = await _mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Gets a specific team member by ID.
    /// </summary>
    /// <param name="id">The team member ID.</param>
    /// <returns>The team member details.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
    public async Task<IActionResult> GetTeamMemberById(Guid id)
    {
        var query = new GetTeamMemberByIdQuery { TeamMemberId = id };
        var result = await _mediator.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Invites a new team member to the organization.
    /// </summary>
    /// <param name="request">The invitation request.</param>
    /// <returns>The created team member.</returns>
    [HttpPost("invite")]
    [Authorize(Policy = PolicyNames.ProviderOwnerOnly)]
    public async Task<IActionResult> InviteTeamMember([FromBody] InviteTeamMemberRequest request)
    {
        var command = new InviteTeamMemberCommand
        {
            Email = request.Email,
            Role = request.Role,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Message = request.Message
        };

        var result = await _mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Updates a team member's role.
    /// </summary>
    /// <param name="id">The team member ID.</param>
    /// <param name="request">The role update request.</param>
    /// <returns>The updated team member.</returns>
    [HttpPut("{id}/role")]
    [Authorize(Policy = PolicyNames.ProviderOwnerOnly)]
    public async Task<IActionResult> UpdateTeamMemberRole(Guid id, [FromBody] UpdateTeamMemberRoleRequest request)
    {
        var command = new UpdateTeamMemberRoleCommand
        {
            TeamMemberId = id,
            NewRole = request.Role
        };

        var result = await _mediator.Send(command);

        return HandleResult(result);
    }

    /// <summary>
    /// Removes a team member from the organization.
    /// </summary>
    /// <param name="id">The team member ID to remove.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyNames.ProviderOwnerOnly)]
    public async Task<IActionResult> RemoveTeamMember(Guid id)
    {
        var command = new RemoveTeamMemberCommand
        {
            TeamMemberId = id
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return NoContent();
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Accepts a team member invitation using an invitation token.
    /// </summary>
    /// <param name="request">The invitation acceptance request.</param>
    /// <returns>The team member with accepted invitation.</returns>
    [HttpPost("accept")]
    [Authorize] // Requires authentication but not specific role (invited user may not have role yet)
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        var command = new AcceptInvitationCommand
        {
            Token = request.Token
        };

        var result = await _mediator.Send(command);

        return HandleResult(result);
    }
}

/// <summary>
/// Request model for inviting a team member.
/// </summary>
public class InviteTeamMemberRequest
{
    /// <summary>
    /// Email address of the person to invite.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Role to assign to the team member.
    /// </summary>
    public required TeamMemberRole Role { get; set; }

    /// <summary>
    /// First name of the person to invite.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the person to invite.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Optional message to include in the invitation.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Request model for accepting a team member invitation.
/// </summary>
public class AcceptInvitationRequest
{
    /// <summary>
    /// The invitation token received via email or link.
    /// </summary>
    public required string Token { get; set; }
}

/// <summary>
/// Request model for updating a team member's role.
/// </summary>
public class UpdateTeamMemberRoleRequest
{
    /// <summary>
    /// New role to assign to the team member.
    /// </summary>
    public required TeamMemberRole Role { get; set; }
}
