using FluentValidation;

namespace Orbito.Application.Features.Payments.Commands.CreatePaymentIntent
{
    /// <summary>
    /// Validator for CreatePaymentIntentCommand
    /// </summary>
    public class CreatePaymentIntentCommandValidator : AbstractValidator<CreatePaymentIntentCommand>
    {
        public CreatePaymentIntentCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("Subscription ID is required");
        }
    }
}
