using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.RenewSubscription
{
    public class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
    {
        public RenewSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be 3 characters long");
        }
    }
}
