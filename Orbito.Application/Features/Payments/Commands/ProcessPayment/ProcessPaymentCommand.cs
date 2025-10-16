using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid SubscriptionId,
    Guid ClientId,
    decimal Amount,
    string Currency,
    string? ExternalTransactionId = null,
    string? PaymentMethod = null,
    string? ExternalPaymentId = null
) : IRequest<Result<PaymentDto>>;
