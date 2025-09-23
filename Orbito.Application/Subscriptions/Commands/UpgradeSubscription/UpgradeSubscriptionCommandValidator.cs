using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.UpgradeSubscription
{
    public class UpgradeSubscriptionCommandValidator : AbstractValidator<UpgradeSubscriptionCommand>
    {
        public UpgradeSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");

            RuleFor(x => x.NewPlanId)
                .NotEmpty()
                .WithMessage("New plan ID is required");

            RuleFor(x => x.NewAmount)
                .GreaterThan(0)
                .WithMessage("New amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be 3 characters long");
        }
    }
}
