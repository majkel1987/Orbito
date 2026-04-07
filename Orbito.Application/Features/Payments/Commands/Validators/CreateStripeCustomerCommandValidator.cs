using FluentValidation;
using Orbito.Application.Features.Payments.Commands;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for create Stripe customer command.
/// Validates client ID, email, name fields, company name, and phone number.
/// </summary>
public class CreateStripeCustomerCommandValidator : AbstractValidator<CreateStripeCustomerCommand>
{
    public CreateStripeCustomerCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(320)
            .WithMessage("Email cannot exceed 320 characters");

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-'\.]+$")
            .WithMessage("First name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-'\.]+$")
            .WithMessage("Last name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.CompanyName)
            .MaximumLength(200)
            .WithMessage("Company name cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9ąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-'\.&,()]+$")
            .WithMessage("Company name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be a valid international format")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}