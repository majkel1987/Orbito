using FluentValidation;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.TeamMembers.Commands.InviteTeamMember;

/// <summary>
/// Validator for InviteTeamMemberCommand.
/// </summary>
public class InviteTeamMemberCommandValidator : AbstractValidator<InviteTeamMemberCommand>
{
    public InviteTeamMemberCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required.")
            .MaximumLength(256)
            .WithMessage("Email address must not exceed 256 characters.")
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("A valid team member role is required.")
            .Must(role => role != TeamMemberRole.Owner)
            .WithMessage("Cannot assign Owner role via invitation. Use ownership transfer instead.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.")
            .When(x => x.LastName != null);

        RuleFor(x => x.Message)
            .MaximumLength(1000)
            .WithMessage("Invitation message must not exceed 1000 characters.")
            .When(x => x.Message != null);
    }
}
