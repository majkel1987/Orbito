using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.ResumeSubscription
{
    public class ResumeSubscriptionCommandValidator : AbstractValidator<ResumeSubscriptionCommand>
    {
        public ResumeSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");
        }
    }
}
