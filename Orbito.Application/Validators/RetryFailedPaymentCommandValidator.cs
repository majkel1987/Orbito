using FluentValidation;
using Orbito.Application.Features.Payments.Commands;

namespace Orbito.Application.Validators
{
    /// <summary>
    /// Validator for RetryFailedPaymentCommand
    /// </summary>
    public class RetryFailedPaymentCommandValidator : AbstractValidator<RetryFailedPaymentCommand>
    {
        public RetryFailedPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment ID is required");

            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));
        }
    }
}
