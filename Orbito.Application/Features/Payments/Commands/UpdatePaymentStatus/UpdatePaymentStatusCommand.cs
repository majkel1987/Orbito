using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public record UpdatePaymentStatusCommand(
        Guid PaymentId,
        Guid ClientId,
        PaymentStatus Status,
        string? FailureReason = null,
        string? RefundReason = null,
        Money? RefundedAmount = null
    ) : IRequest<Result<PaymentDto>>;
}
