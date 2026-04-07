using FluentValidation;

namespace Orbito.Application.Providers.Commands.DeleteProvider;

public class DeleteProviderCommandValidator : AbstractValidator<DeleteProviderCommand>
{
    public DeleteProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provider ID is required");
    }
}
