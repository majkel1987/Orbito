using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.ActivateSubscription
{
    public class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
    {
        public ActivateSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");
        }
    }
}
