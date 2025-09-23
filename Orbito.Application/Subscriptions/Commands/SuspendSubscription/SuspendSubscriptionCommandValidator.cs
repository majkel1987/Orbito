using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.SuspendSubscription
{
    public class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
    {
        public SuspendSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters");
        }
    }
}
