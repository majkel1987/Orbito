using FluentValidation;
using Orbito.Domain.Enums;

namespace Orbito.Application.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Validator for CreateSubscriptionCommand.
/// Validates ClientId, PlanId, Amount, Currency, BillingPeriod, and TrialDays.
/// </summary>
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
            .WithMessage("Currency must be 3 characters long")
            .Must(BeValidIso4217Currency)
            .WithMessage("Currency must be a valid ISO 4217 code (e.g., USD, EUR, PLN, GBP)");

        RuleFor(x => x.BillingPeriodValue)
            .GreaterThan(0)
            .WithMessage("Billing period value must be greater than 0");

        RuleFor(x => x.BillingPeriodType)
            .NotEmpty()
            .WithMessage("Billing period type is required")
            .Must(type => Enum.TryParse<BillingPeriodType>(type, ignoreCase: true, out _))
            .WithMessage("Invalid billing period type. Must be Daily, Weekly, Monthly, or Yearly");

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Trial days must be 0 or greater");
    }

    /// <summary>
    /// Supported ISO 4217 currency codes.
    /// </summary>
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "PLN", "GBP", "JPY", "CHF", "AUD", "CAD", "CNY", "SEK",
        "NOK", "DKK", "CZK", "HUF", "RON", "BGN", "HRK", "ISK", "TRY", "RUB",
        "INR", "BRL", "MXN", "ZAR", "KRW", "SGD", "HKD", "NZD", "THB", "MYR"
    };

    private static bool BeValidIso4217Currency(string currency)
        => ValidCurrencies.Contains(currency);
}
