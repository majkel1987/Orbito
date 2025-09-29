using Orbito.Domain.Enums;

namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Wynik przetwarzania płatności przez payment gateway
    /// </summary>
    public record PaymentResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// Status płatności
        /// </summary>
        public required PaymentStatus Status { get; init; }

        /// <summary>
        /// Zewnętrzny ID płatności w payment gateway
        /// </summary>
        public string? ExternalPaymentId { get; init; }

        /// <summary>
        /// ID transakcji w payment gateway
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
        /// URL do przekierowania (dla niektórych payment gateway)
        /// </summary>
        public string? RedirectUrl { get; init; }

        /// <summary>
        /// Metadane odpowiedzi
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Data utworzenia płatności w payment gateway
        /// </summary>
        public DateTime? CreatedAt { get; init; }

        /// <summary>
        /// Data zakończenia płatności w payment gateway
        /// </summary>
        public DateTime? CompletedAt { get; init; }

        /// <summary>
        /// Metoda płatności użyta w payment gateway
        /// </summary>
        public string? PaymentMethod { get; init; }

        /// <summary>
        /// Ostatnie 4 cyfry karty (jeśli dotyczy)
        /// </summary>
        public string? LastFourDigits { get; init; }

        /// <summary>
        /// Marka karty (jeśli dotyczy)
        /// </summary>
        public string? CardBrand { get; init; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static PaymentResult Success(
            PaymentStatus status,
            string externalPaymentId,
            string? transactionId = null,
            string? redirectUrl = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null,
            DateTime? completedAt = null,
            string? paymentMethod = null,
            string? lastFourDigits = null,
            string? cardBrand = null)
        {
            return new PaymentResult
            {
                IsSuccess = true,
                Status = status,
                ExternalPaymentId = externalPaymentId,
                TransactionId = transactionId,
                RedirectUrl = redirectUrl,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow,
                CompletedAt = completedAt,
                PaymentMethod = paymentMethod,
                LastFourDigits = lastFourDigits,
                CardBrand = cardBrand
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static PaymentResult Failure(
            string errorMessage,
            string? errorCode = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }
    }
}
