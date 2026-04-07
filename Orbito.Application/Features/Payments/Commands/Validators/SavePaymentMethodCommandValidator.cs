using FluentValidation;
using Orbito.Application.Features.Payments.Commands.SavePaymentMethod;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for SavePaymentMethodCommand.
/// Validates client ID, payment method type, token, card details, and metadata.
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

        // FIXED: Added null check for Metadata to prevent NullReferenceException
        RuleFor(x => x.Metadata)
            .Must(metadata => metadata == null || metadata.Count <= 20)
            .WithMessage("Metadata cannot contain more than 20 entries");

        // Only validate metadata keys/values if metadata is not null and has entries
        When(x => x.Metadata != null && x.Metadata.Any(), () =>
        {
            RuleForEach(x => x.Metadata.Keys)
                .MaximumLength(50)
                .WithMessage("Metadata key cannot exceed 50 characters");

            RuleForEach(x => x.Metadata.Values)
                .MaximumLength(500)
                .WithMessage("Metadata value cannot exceed 500 characters");
        });
    }
}
