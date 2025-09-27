using MediatR;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public record UpdatePaymentStatusCommand(
        Guid PaymentId,
        PaymentStatus Status,
        string? FailureReason = null,
        string? RefundReason = null
    ) : IRequest<UpdatePaymentStatusResult>;
}
