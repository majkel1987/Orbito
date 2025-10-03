using MediatR;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public record UpdatePaymentStatusCommand(
        Guid PaymentId,
        PaymentStatus Status,
        string? FailureReason = null,
        string? RefundReason = null,
        Money? RefundedAmount = null
    ) : IRequest<UpdatePaymentStatusResult>;
}
