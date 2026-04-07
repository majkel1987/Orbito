using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription;

/// <summary>
/// Validator for ActivateSubscriptionCommand.
/// </summary>
public class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}
