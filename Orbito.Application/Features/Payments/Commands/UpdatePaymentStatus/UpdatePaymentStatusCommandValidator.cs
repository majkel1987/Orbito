using FluentValidation;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;

/// <summary>
/// Validator for UpdatePaymentStatusCommand.
/// Performs input validation (format, required fields).
/// Business rule validation (status transitions) is handled in the handler.
/// </summary>
public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    private static readonly PaymentStatus[] ValidTargetStatuses =
    [
        PaymentStatus.Processing,
        PaymentStatus.Completed,
        PaymentStatus.Failed,
        PaymentStatus.Cancelled,
        PaymentStatus.Refunded,
        PaymentStatus.PartiallyRefunded
    ];

    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.Status)
            .Must(status => ValidTargetStatuses.Contains(status))
            .WithMessage("Invalid target payment status. Cannot transition to Pending status.");

        RuleFor(x => x.FailureReason)
            .NotEmpty()
            .When(x => x.Status == PaymentStatus.Failed)
            .WithMessage("Failure reason is required when marking payment as failed");

        RuleFor(x => x.FailureReason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.FailureReason))
            .WithMessage("Failure reason cannot exceed 1000 characters");

        RuleFor(x => x.RefundReason)
            .NotEmpty()
            .When(x => x.Status == PaymentStatus.Refunded || x.Status == PaymentStatus.PartiallyRefunded)
            .WithMessage("Refund reason is required when marking payment as refunded");

        RuleFor(x => x.RefundReason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.RefundReason))
            .WithMessage("Refund reason cannot exceed 1000 characters");

        RuleFor(x => x.RefundedAmount)
            .NotNull()
            .When(x => x.Status == PaymentStatus.PartiallyRefunded)
            .WithMessage("Refunded amount is required for partial refunds");

        RuleFor(x => x.RefundedAmount!.Amount)
            .GreaterThan(0)
            .When(x => x.Status == PaymentStatus.PartiallyRefunded && x.RefundedAmount != null)
            .WithMessage("Refunded amount must be greater than zero");
    }
}
