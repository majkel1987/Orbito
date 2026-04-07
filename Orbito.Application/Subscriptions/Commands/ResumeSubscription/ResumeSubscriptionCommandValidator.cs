using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription;

/// <summary>
/// Validator for ResumeSubscriptionCommand.
/// </summary>
public class ResumeSubscriptionCommandValidator : AbstractValidator<ResumeSubscriptionCommand>
{
    public ResumeSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");
    }
}
