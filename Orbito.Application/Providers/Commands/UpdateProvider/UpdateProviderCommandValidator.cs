using FluentValidation;

namespace Orbito.Application.Providers.Commands.UpdateProvider;

public class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provider ID is required");

        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .WithMessage("Business name is required")
            .MaximumLength(200)
            .WithMessage("Business name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Avatar)
            .MaximumLength(500)
            .WithMessage("Avatar URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Avatar));

        RuleFor(x => x.SubdomainSlug)
            .MaximumLength(50)
            .WithMessage("Subdomain slug cannot exceed 50 characters")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Subdomain slug can only contain lowercase letters, numbers, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.SubdomainSlug));

        RuleFor(x => x.CustomDomain)
            .MaximumLength(255)
            .WithMessage("Custom domain cannot exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]?\.[a-zA-Z]{2,}$")
            .WithMessage("Custom domain must be a valid domain name")
            .When(x => !string.IsNullOrEmpty(x.CustomDomain));
    }
}
