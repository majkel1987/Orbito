using FluentValidation;

namespace Orbito.Application.Subscriptions.Commands.UpgradeSubscription;

/// <summary>
/// Validator for UpgradeSubscriptionCommand.
/// </summary>
public class UpgradeSubscriptionCommandValidator : AbstractValidator<UpgradeSubscriptionCommand>
{
    public UpgradeSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("Subscription ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.NewPlanId)
            .NotEmpty()
            .WithMessage("New plan ID is required");

        RuleFor(x => x.NewAmount)
            .GreaterThan(0)
            .WithMessage("New amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be 3 characters long")
            .Must(BeValidIso4217Currency)
            .WithMessage("Currency must be a valid ISO 4217 code (e.g., USD, EUR, PLN, GBP)");
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
