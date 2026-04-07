using FluentValidation;

namespace Orbito.Application.Features.TeamMembers.Commands.RemoveTeamMember;

/// <summary>
/// Validator for RemoveTeamMemberCommand.
/// </summary>
public class RemoveTeamMemberCommandValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamMemberId)
            .NotEmpty()
            .WithMessage("Team member ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason != null);
    }
}
