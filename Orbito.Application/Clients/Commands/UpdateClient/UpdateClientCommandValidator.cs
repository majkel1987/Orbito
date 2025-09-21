using FluentValidation;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
    {
        public UpdateClientCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Client ID is required");

            RuleFor(x => x.DirectEmail)
                .EmailAddress()
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.DirectEmail))
                .WithMessage("Email must be valid and not exceed 255 characters");

            RuleFor(x => x.DirectFirstName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.DirectFirstName))
                .WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.DirectLastName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.DirectLastName))
                .WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.CompanyName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
                .WithMessage("Company name must not exceed 200 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone number must be valid and not exceed 20 characters");

            // Sprawdź czy przynajmniej jedno pole do aktualizacji jest podane
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.CompanyName) ||
                           !string.IsNullOrWhiteSpace(x.Phone) ||
                           !string.IsNullOrWhiteSpace(x.DirectEmail) ||
                           !string.IsNullOrWhiteSpace(x.DirectFirstName) ||
                           !string.IsNullOrWhiteSpace(x.DirectLastName))
                .WithMessage("At least one field must be provided for update");
        }
    }
}
