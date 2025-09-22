using FluentValidation;

namespace Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan
{
    public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
    {
        public UpdateSubscriptionPlanCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Plan ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Plan name is required")
                .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be 3 characters (ISO 4217)");

            RuleFor(x => x.BillingPeriodType)
                .IsInEnum().WithMessage("Invalid billing period type");

            RuleFor(x => x.TrialDays)
                .GreaterThanOrEqualTo(0).WithMessage("Trial days must be greater than or equal to 0");

            RuleFor(x => x.TrialPeriodDays)
                .GreaterThanOrEqualTo(0).WithMessage("Trial period days must be greater than or equal to 0");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be greater than or equal to 0");
        }
    }
}
