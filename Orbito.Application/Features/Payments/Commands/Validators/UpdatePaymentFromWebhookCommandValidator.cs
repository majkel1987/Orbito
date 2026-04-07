using FluentValidation;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentFromWebhook;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.Validators;

/// <summary>
/// Validator for UpdatePaymentFromWebhookCommand
/// </summary>
public class UpdatePaymentFromWebhookCommandValidator : AbstractValidator<UpdatePaymentFromWebhookCommand>
{
    public UpdatePaymentFromWebhookCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required for idempotency")
            .MaximumLength(200)
            .WithMessage("Event ID cannot exceed 200 characters");

        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("Event type is required")
            .MaximumLength(100)
            .WithMessage("Event type cannot exceed 100 characters");

        RuleFor(x => x.Payload)
            .NotEmpty()
            .WithMessage("Payload is required")
            .MaximumLength(1048576) // 1 MB limit
            .WithMessage("Payload cannot exceed 1 MB");

        RuleFor(x => x.ExternalPaymentId)
            .NotEmpty()
            .WithMessage("External payment ID is required")
            .MaximumLength(200)
            .WithMessage("External payment ID cannot exceed 200 characters");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid payment status");

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000)
            .WithMessage("Error message cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ErrorMessage));

        // Null-safe metadata validation
        RuleFor(x => x.Metadata)
            .Must(metadata => metadata == null || metadata.Count <= 50)
            .WithMessage("Metadata cannot contain more than 50 entries");

        // Only validate keys/values if metadata is not null and has entries
        When(x => x.Metadata != null && x.Metadata.Any(), () =>
        {
            RuleForEach(x => x.Metadata.Keys)
                .MaximumLength(100)
                .WithMessage("Metadata key cannot exceed 100 characters");

            RuleForEach(x => x.Metadata.Values)
                .MaximumLength(1000)
                .WithMessage("Metadata value cannot exceed 1000 characters");
        });
    }
}
