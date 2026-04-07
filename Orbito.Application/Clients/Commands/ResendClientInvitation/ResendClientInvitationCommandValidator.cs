using FluentValidation;

namespace Orbito.Application.Clients.Commands.ResendClientInvitation;

/// <summary>
/// Validator for ResendClientInvitationCommand.
/// Validates that a valid ClientId is provided.
/// </summary>
public class ResendClientInvitationCommandValidator : AbstractValidator<ResendClientInvitationCommand>
{
    public ResendClientInvitationCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Client ID must be a valid GUID.");
    }
}
