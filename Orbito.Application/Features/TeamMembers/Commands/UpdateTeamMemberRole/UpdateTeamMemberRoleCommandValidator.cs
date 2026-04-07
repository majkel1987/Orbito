using FluentValidation;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.TeamMembers.Commands.UpdateTeamMemberRole;

/// <summary>
/// Validator for UpdateTeamMemberRoleCommand.
/// </summary>
public class UpdateTeamMemberRoleCommandValidator : AbstractValidator<UpdateTeamMemberRoleCommand>
{
    public UpdateTeamMemberRoleCommandValidator()
    {
        RuleFor(x => x.TeamMemberId)
            .NotEmpty()
            .WithMessage("Team member ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage("A valid team member role is required.")
            .Must(role => role != TeamMemberRole.Owner)
            .WithMessage("Cannot assign Owner role directly. Use ownership transfer instead.");
    }
}
