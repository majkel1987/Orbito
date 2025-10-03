using FluentValidation;
using Orbito.Application.Features.Payments.Commands.SavePaymentMethod;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for SavePaymentMethodCommand
/// </summary>
public class SavePaymentMethodCommandValidator : AbstractValidator<SavePaymentMethodCommand>
{
    public SavePaymentMethodCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid payment method type");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Payment token is required")
            .MinimumLength(10)
            .WithMessage("Payment token must be at least 10 characters")
            .MaximumLength(500)
            .WithMessage("Payment token cannot exceed 500 characters");

        RuleFor(x => x.LastFourDigits)
            .Matches(@"^\d{4}$")
            .When(x => !string.IsNullOrEmpty(x.LastFourDigits))
            .WithMessage("Last four digits must be exactly 4 numeric characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future");

        // Validate that card types have expiry date
        RuleFor(x => x.ExpiryDate)
            .NotNull()
            .When(x => x.Type == PaymentMethodType.Card)
            .WithMessage("Expiry date is required for cards");

        RuleFor(x => x.Metadata)
            .Must(metadata => metadata.Count <= 20)
            .When(x => x.Metadata.Any())
            .WithMessage("Metadata cannot contain more than 20 entries");

        RuleForEach(x => x.Metadata.Keys)
            .MaximumLength(50)
            .WithMessage("Metadata key cannot exceed 50 characters");

        RuleForEach(x => x.Metadata.Values)
            .MaximumLength(500)
            .WithMessage("Metadata value cannot exceed 500 characters");
    }
}
