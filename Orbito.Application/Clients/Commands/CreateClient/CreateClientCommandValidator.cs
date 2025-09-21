using FluentValidation;

namespace Orbito.Application.Clients.Commands.CreateClient
{
    public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public CreateClientCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.DirectEmail))
                .WithMessage("Either UserId or DirectEmail must be provided");

            RuleFor(x => x.DirectEmail)
                .NotEmpty()
                .EmailAddress()
                .When(x => !x.UserId.HasValue)
                .WithMessage("Valid email is required when UserId is not provided");

            RuleFor(x => x.DirectFirstName)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => !x.UserId.HasValue)
                .WithMessage("First name is required and must not exceed 100 characters when UserId is not provided");

            RuleFor(x => x.DirectLastName)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => !x.UserId.HasValue)
                .WithMessage("Last name is required and must not exceed 100 characters when UserId is not provided");

            RuleFor(x => x.CompanyName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName))
                .WithMessage("Company name must not exceed 200 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Phone number must be valid and not exceed 20 characters");

            // Walidacja wzajemnego wykluczania
            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrWhiteSpace(x.DirectEmail))
                .WithMessage("Either UserId or DirectEmail must be provided");

            RuleFor(x => x)
                .Must(x => !x.UserId.HasValue || string.IsNullOrWhiteSpace(x.DirectEmail))
                .WithMessage("Cannot provide both UserId and DirectEmail");
        }
    }
}
