using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Request do przetwarzania płatności przez payment gateway
    /// </summary>
    public record ProcessPaymentRequest
    {
        /// <summary>
        /// ID płatności w systemie
        /// </summary>
        public required Guid PaymentId { get; init; }

        /// <summary>
        /// ID subskrypcji
        /// </summary>
        public required Guid SubscriptionId { get; init; }

        /// <summary>
        /// ID klienta
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Kwota płatności
        /// </summary>
        public required Money Amount { get; init; }

        /// <summary>
        /// ID metody płatności (token z payment gateway)
        /// </summary>
        public required string PaymentMethodId { get; init; }

        /// <summary>
        /// Opis płatności
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// Metadane płatności
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// URL powrotu po udanej płatności
        /// </summary>
        public string? SuccessUrl { get; init; }

        /// <summary>
        /// URL powrotu po nieudanej płatności
        /// </summary>
        public string? CancelUrl { get; init; }

        /// <summary>
        /// ID tenanta dla multi-tenancy
        /// </summary>
        public required Guid TenantId { get; init; }

        /// <summary>
        /// Klucz idempotentności dla bezpiecznych powtórzeń
        /// </summary>
        public required string IdempotencyKey { get; init; }

        /// <summary>
        /// Walidacja request'u
        /// </summary>
        public void Validate()
        {
            if (PaymentId == Guid.Empty)
                throw new ArgumentException("PaymentId cannot be empty");

            if (SubscriptionId == Guid.Empty)
                throw new ArgumentException("SubscriptionId cannot be empty");

            if (ClientId == Guid.Empty)
                throw new ArgumentException("ClientId cannot be empty");

            if (Amount.Amount <= 0)
                throw new ArgumentException("Amount must be positive");

            if (string.IsNullOrWhiteSpace(PaymentMethodId))
                throw new ArgumentException("PaymentMethodId is required");

            if (string.IsNullOrWhiteSpace(Description))
                throw new ArgumentException("Description is required");

            if (TenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty");

            if (string.IsNullOrWhiteSpace(IdempotencyKey))
                throw new ArgumentException("IdempotencyKey is required");
        }
    }
}
