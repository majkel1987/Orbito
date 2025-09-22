using FluentValidation;

namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan
{
    public class CloneSubscriptionPlanCommandValidator : AbstractValidator<CloneSubscriptionPlanCommand>
    {
        public CloneSubscriptionPlanCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Original plan ID is required");

            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("New plan name is required")
                .MaximumLength(200).WithMessage("New plan name cannot exceed 200 characters");

            RuleFor(x => x.NewDescription)
                .MaximumLength(1000).WithMessage("New description cannot exceed 1000 characters");

            RuleFor(x => x.NewAmount)
                .GreaterThanOrEqualTo(0).When(x => x.NewAmount.HasValue)
                .WithMessage("New amount must be greater than or equal to 0");

            RuleFor(x => x.NewCurrency)
                .Length(3).When(x => !string.IsNullOrEmpty(x.NewCurrency))
                .WithMessage("New currency must be 3 characters (ISO 4217)");

            RuleFor(x => x.NewSortOrder)
                .GreaterThanOrEqualTo(0).When(x => x.NewSortOrder.HasValue)
                .WithMessage("New sort order must be greater than or equal to 0");
        }
    }
}
