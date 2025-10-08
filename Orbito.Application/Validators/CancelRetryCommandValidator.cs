using FluentValidation;
using Orbito.Application.Features.Payments.Commands;

namespace Orbito.Application.Validators
{
    /// <summary>
    /// Validator for CancelRetryCommand
    /// </summary>
    public class CancelRetryCommandValidator : AbstractValidator<CancelRetryCommand>
    {
        public CancelRetryCommandValidator()
        {
            RuleFor(x => x.ScheduleId)
                .NotEmpty()
                .WithMessage("Schedule ID is required");

            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required");
        }
    }
}
