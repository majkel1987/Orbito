using FluentValidation;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.CreateProviderPaymentIntent
{
    /// <summary>
    /// Validator for CreateProviderPaymentIntentCommand.
    /// PlatformPlanId is optional (null = use current plan).
    /// </summary>
    public class CreateProviderPaymentIntentCommandValidator : AbstractValidator<CreateProviderPaymentIntentCommand>
    {
        public CreateProviderPaymentIntentCommandValidator()
        {
            When(x => x.PlatformPlanId.HasValue, () =>
            {
                RuleFor(x => x.PlatformPlanId!.Value)
                    .NotEqual(Guid.Empty)
                    .WithMessage("PlatformPlanId must be a valid GUID if provided");
            });
        }
    }
}
