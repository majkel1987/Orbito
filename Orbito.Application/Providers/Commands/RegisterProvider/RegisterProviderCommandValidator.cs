using FluentValidation;

namespace Orbito.Application.Providers.Commands.RegisterProvider
{
    public class RegisterProviderCommandValidator : AbstractValidator<RegisterProviderCommand>
    {
        public RegisterProviderCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .MaximumLength(200).WithMessage("Business name must not exceed 200 characters");

            RuleFor(x => x.SubdomainSlug)
                .NotEmpty().WithMessage("Subdomain slug is required")
                .MaximumLength(50).WithMessage("Subdomain slug must not exceed 50 characters")
                .Matches(@"^[a-z0-9-]+$").WithMessage("Subdomain slug may only contain lowercase letters, numbers and hyphens");
        }
    }
}
