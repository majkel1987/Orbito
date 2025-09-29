using FluentValidation;
using Orbito.Application.Features.Payments.Commands;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Walidator dla komendy zwrotu płatności
/// </summary>
public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Refund amount must be greater than zero")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Refund amount cannot exceed 1,000,000")
            .PrecisionScale(10, 2, true)
            .WithMessage("Invalid amount format (max 2 decimal places)")
            .Must(amount => amount >= 0.50m)
            .WithMessage("Minimum refund amount is 0.50");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter code")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter uppercase code");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required")
            .MaximumLength(500)
            .WithMessage("Refund reason cannot exceed 500 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_,.()]+$")
            .WithMessage("Refund reason contains invalid characters");
    }
}