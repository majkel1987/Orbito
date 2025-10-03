using FluentValidation;
using Orbito.Application.Features.Payments.Commands.ProcessWebhookEvent;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for ProcessWebhookEventCommand
/// </summary>
public class ProcessWebhookEventCommandValidator : AbstractValidator<ProcessWebhookEventCommand>
{
    public ProcessWebhookEventCommandValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("Event type is required")
            .MaximumLength(100)
            .WithMessage("Event type cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9._\-]+$")
            .WithMessage("Event type contains invalid characters");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required")
            .MaximumLength(200)
            .WithMessage("Event ID cannot exceed 200 characters");

        RuleFor(x => x.Payload)
            .NotEmpty()
            .WithMessage("Payload is required")
            .MaximumLength(1048576) // 1 MB limit
            .WithMessage("Payload cannot exceed 1 MB");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("Signature is required")
            .MaximumLength(500)
            .WithMessage("Signature cannot exceed 500 characters");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required")
            .MaximumLength(50)
            .WithMessage("Provider name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("Provider name must be alphanumeric");
    }
}
