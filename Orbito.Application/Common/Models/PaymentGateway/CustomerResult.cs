namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Wynik tworzenia klienta w payment gateway
    /// </summary>
    public record CustomerResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// Zewnętrzny ID klienta w payment gateway
        /// </summary>
        public string? ExternalCustomerId { get; init; }

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
        /// Data utworzenia klienta w payment gateway
        /// </summary>
        public DateTime? CreatedAt { get; init; }

        /// <summary>
        /// Email klienta (required for success)
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Imię klienta
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Nazwisko klienta
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static CustomerResult Success(
            string externalCustomerId,
            string email,
            string? firstName = null,
            string? lastName = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null)
        {
            if (string.IsNullOrWhiteSpace(externalCustomerId))
                throw new ArgumentException("ExternalCustomerId is required for success");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required for success");

            return new CustomerResult
            {
                IsSuccess = true,
                ExternalCustomerId = externalCustomerId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static CustomerResult Failure(
            string errorMessage,
            string? errorCode = null,
            Dictionary<string, string>? metadata = null,
            DateTime? createdAt = null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("ErrorMessage is required for failure");

            return new CustomerResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }
    }
}
