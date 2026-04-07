using FluentValidation;

namespace Orbito.Application.Features.TeamMembers.Commands.AcceptInvitation;

/// <summary>
/// Validator for AcceptInvitationCommand.
/// </summary>
public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Invitation token is required.")
            .MaximumLength(500)
            .WithMessage("Invitation token must not exceed 500 characters.");
    }
}
