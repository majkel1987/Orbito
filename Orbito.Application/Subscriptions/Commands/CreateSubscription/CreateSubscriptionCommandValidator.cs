using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
    {
        public CreateSubscriptionCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required");

            RuleFor(x => x.PlanId)
                .NotEmpty()
                .WithMessage("Plan ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be 3 characters long");

            RuleFor(x => x.BillingPeriodValue)
                .GreaterThan(0)
                .WithMessage("Billing period value must be greater than 0");

            RuleFor(x => x.BillingPeriodType)
                .NotEmpty()
                .WithMessage("Billing period type is required")
                .Must(BeValidBillingPeriodType)
                .WithMessage("Invalid billing period type. Must be Daily, Weekly, Monthly, or Yearly");

            RuleFor(x => x.TrialDays)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Trial days must be 0 or greater");
        }

        private static bool BeValidBillingPeriodType(string type)
        {
            return type.Equals("Daily", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Weekly", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Yearly", StringComparison.OrdinalIgnoreCase);
        }
    }
}
