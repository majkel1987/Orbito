using FluentValidation;

namespace Orbito.Application.Providers.Commands.CreateProvider;

/// <summary>
/// Validator for CreateProviderCommand.
/// Validates all required fields for creating a new provider.
/// </summary>
public class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .WithMessage("Business name is required")
            .MaximumLength(200)
            .WithMessage("Business name cannot exceed 200 characters");

        RuleFor(x => x.SubdomainSlug)
            .NotEmpty()
            .WithMessage("Subdomain slug is required")
            .MaximumLength(50)
            .WithMessage("Subdomain slug cannot exceed 50 characters")
            .Matches(@"^[a-z0-9][a-z0-9-]*[a-z0-9]$|^[a-z0-9]$")
            .WithMessage("Subdomain slug must start and end with a letter or number, and may only contain lowercase letters, numbers, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(256)
            .WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Avatar)
            .MaximumLength(500)
            .WithMessage("Avatar URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Avatar));

        RuleFor(x => x.CustomDomain)
            .MaximumLength(255)
            .WithMessage("Custom domain cannot exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]?\.[a-zA-Z]{2,}$")
            .WithMessage("Custom domain must be a valid domain name")
            .When(x => !string.IsNullOrEmpty(x.CustomDomain));
    }
}
