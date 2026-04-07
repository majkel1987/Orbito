using FluentValidation;

namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public class DeleteSubscriptionPlanCommandValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID is required");
    }
}
