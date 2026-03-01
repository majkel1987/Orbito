using FluentValidation;

namespace Orbito.Application.Clients.Commands.InviteClient;

public class InviteClientCommandValidator : AbstractValidator<InviteClientCommand>
{
    public InviteClientCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyName));
    }
}
