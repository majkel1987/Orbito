using FluentValidation;

namespace Orbito.Application.Clients.Commands.ActivateClient;

public class ActivateClientCommandValidator : AbstractValidator<ActivateClientCommand>
{
    public ActivateClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}
