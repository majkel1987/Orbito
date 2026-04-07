using FluentValidation;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan;

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
            .WithMessage("New currency must be 3 characters (ISO 4217)")
            .Must(currency => Currency.IsSupported(currency!))
            .When(x => !string.IsNullOrEmpty(x.NewCurrency))
            .WithMessage("Currency code is not supported. Supported currencies: USD, EUR, GBP, PLN, CAD, AUD, JPY, CHF, SEK, NOK, DKK, CZK, HUF, RON, BGN");

        RuleFor(x => x.NewSortOrder)
            .GreaterThanOrEqualTo(0).When(x => x.NewSortOrder.HasValue)
            .WithMessage("New sort order must be greater than or equal to 0");

        // Cross-field validation: if NewAmount is specified, NewCurrency should also be specified (or both null)
        RuleFor(x => x)
            .Must(x => (x.NewAmount.HasValue && !string.IsNullOrEmpty(x.NewCurrency)) || (!x.NewAmount.HasValue && string.IsNullOrEmpty(x.NewCurrency)))
            .WithMessage("NewAmount and NewCurrency must both be specified or both be null")
            .When(x => x.NewAmount.HasValue || !string.IsNullOrEmpty(x.NewCurrency));
    }
}
