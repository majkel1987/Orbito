using Orbito.Domain.Enums;

namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Wynik zwrotu płatności przez payment gateway
    /// </summary>
    public record RefundResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// Status zwrotu
        /// </summary>
        public required RefundStatus Status { get; init; }

        /// <summary>
        /// Zewnętrzny ID zwrotu w payment gateway
        /// </summary>
        public string? ExternalRefundId { get; init; }

        /// <summary>
        /// ID transakcji zwrotu w payment gateway
        /// </summary>
        public string? TransactionId { get; init; }

        /// <summary>
        /// Komunikat błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Kod błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Metadane odpowiedzi
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Data utworzenia zwrotu w payment gateway
        /// </summary>
        public DateTime? CreatedAt { get; init; }

        /// <summary>
        /// Data zakończenia zwrotu w payment gateway
        /// </summary>
        public DateTime? CompletedAt { get; init; }

        /// <summary>
        /// Powód zwrotu
        /// </summary>
        public string? Reason { get; init; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static RefundResult Success(
            RefundStatus status,
            string externalRefundId,
            string? transactionId = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null,
            DateTime? completedAt = null,
            string? reason = null)
        {
            return new RefundResult
            {
                IsSuccess = true,
                Status = status,
                ExternalRefundId = externalRefundId,
                TransactionId = transactionId,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow,
                CompletedAt = completedAt,
                Reason = reason
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static RefundResult Failure(
            string errorMessage,
            string? errorCode = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null)
        {
            return new RefundResult
            {
                IsSuccess = false,
                Status = RefundStatus.Failed,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }
    }
}
