using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Payments.Commands
{
    /// <summary>
    /// Komenda do zwrotu płatności
    /// </summary>
    public record RefundPaymentCommand : IRequest<Result<RefundPaymentResult>>
    {
        /// <summary>
        /// ID płatności
        /// </summary>
        public Guid PaymentId { get; init; }

        /// <summary>
        /// ID klienta (security: ClientId verification)
        /// </summary>
        public Guid ClientId { get; init; }

        /// <summary>
        /// Kwota zwrotu
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Waluta
        /// </summary>
        public string Currency { get; init; } = string.Empty;

        /// <summary>
        /// Powód zwrotu
        /// </summary>
        public string Reason { get; init; } = string.Empty;
    }

    /// <summary>
    /// Wynik zwrotu płatności
    /// </summary>
    public record RefundPaymentResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Komunikat błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Kod błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Zewnętrzny ID zwrotu
        /// </summary>
        public string? ExternalRefundId { get; init; }

        /// <summary>
        /// Status zwrotu
        /// </summary>
        public string? Status { get; init; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static RefundPaymentResult Success(string externalRefundId, string status)
        {
            return new RefundPaymentResult
            {
                IsSuccess = true,
                ExternalRefundId = externalRefundId,
                Status = status
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static RefundPaymentResult Failure(string errorMessage, string? errorCode = null)
        {
            return new RefundPaymentResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
