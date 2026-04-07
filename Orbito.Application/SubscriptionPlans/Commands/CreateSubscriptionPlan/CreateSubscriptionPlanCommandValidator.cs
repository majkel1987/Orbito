using FluentValidation;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan;

public class CreateSubscriptionPlanCommandValidator : AbstractValidator<CreateSubscriptionPlanCommand>
{
    public CreateSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217)")
            .Must(Currency.IsSupported)
            .WithMessage("Currency code is not supported. Supported currencies: USD, EUR, GBP, PLN, CAD, AUD, JPY, CHF, SEK, NOK, DKK, CZK, HUF, RON, BGN");

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
