using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Request do zwrotu płatności przez payment gateway
    /// </summary>
    public record RefundRequest
    {
        /// <summary>
        /// ID płatności w systemie
        /// </summary>
        public required Guid PaymentId { get; init; }

        /// <summary>
        /// Zewnętrzny ID płatności w payment gateway
        /// </summary>
        public required string ExternalPaymentId { get; init; }

        /// <summary>
        /// Kwota zwrotu (może być częściowa)
        /// </summary>
        public required Money Amount { get; init; }

        /// <summary>
        /// Powód zwrotu
        /// </summary>
        public required string Reason { get; init; }

        /// <summary>
        /// Metadane zwrotu
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// ID tenanta dla multi-tenancy
        /// </summary>
        public required Guid TenantId { get; init; }

        /// <summary>
        /// Walidacja request'u
        /// </summary>
        public void Validate()
        {
            if (PaymentId == Guid.Empty)
                throw new ArgumentException("PaymentId cannot be empty");

            if (string.IsNullOrWhiteSpace(ExternalPaymentId))
                throw new ArgumentException("ExternalPaymentId is required");

            if (Amount.Amount <= 0)
                throw new ArgumentException("Amount must be positive");

            if (string.IsNullOrWhiteSpace(Reason))
                throw new ArgumentException("Reason is required");

            if (TenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty");
        }
    }
}
