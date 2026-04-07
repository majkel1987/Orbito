using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    /// <summary>
    /// Command to update payment status
    /// </summary>
    public record UpdatePaymentStatusCommand : IRequest<Result<PaymentDto>>
    {
        /// <summary>
        /// Payment ID
        /// </summary>
        public required Guid PaymentId { get; init; }

        /// <summary>
        /// Client ID
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// New payment status
        /// </summary>
        public required PaymentStatus Status { get; init; }

        /// <summary>
        /// Failure reason if status is Failed
        /// </summary>
        public string? FailureReason { get; init; }

        /// <summary>
        /// Refund reason if status is Refunded
        /// </summary>
        public string? RefundReason { get; init; }

        /// <summary>
        /// Refunded amount if status is Refunded
        /// </summary>
        public Money? RefundedAmount { get; init; }
    }
}
