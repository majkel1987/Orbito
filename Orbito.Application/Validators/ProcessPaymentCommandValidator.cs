using FluentValidation;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;

namespace Orbito.Application.Validators
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");

            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Payment amount must be greater than zero")
                .LessThan(1000000)
                .WithMessage("Payment amount cannot exceed 1,000,000");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be a 3-character code (e.g., USD, EUR, PLN)")
                .Matches(@"^[A-Z]{3}$")
                .WithMessage("Currency must be a valid 3-letter uppercase code");

            RuleFor(x => x.ExternalTransactionId)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.ExternalTransactionId))
                .WithMessage("External transaction ID must not exceed 255 characters");

            RuleFor(x => x.PaymentMethod)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod))
                .WithMessage("Payment method must not exceed 50 characters");

            RuleFor(x => x.ExternalPaymentId)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.ExternalPaymentId))
                .WithMessage("External payment ID must not exceed 255 characters");
        }
    }
}
