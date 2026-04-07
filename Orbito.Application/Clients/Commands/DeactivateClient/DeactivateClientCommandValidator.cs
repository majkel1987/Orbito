using FluentValidation;

namespace Orbito.Application.Clients.Commands.DeactivateClient;

public class DeactivateClientCommandValidator : AbstractValidator<DeactivateClientCommand>
{
    public DeactivateClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}
