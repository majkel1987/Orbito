using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Request for creating a PaymentIntent (for Stripe Elements)
    /// </summary>
    public record CreatePaymentIntentRequest
    {
        /// <summary>
        /// Payment amount
        /// </summary>
        public required Money Amount { get; init; }

        /// <summary>
        /// Stripe Customer ID
        /// </summary>
        public required string CustomerId { get; init; }

        /// <summary>
        /// Subscription ID for metadata
        /// </summary>
        public required Guid SubscriptionId { get; init; }

        /// <summary>
        /// Client ID for metadata
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Tenant ID for metadata
        /// </summary>
        public required Guid TenantId { get; init; }

        /// <summary>
        /// Optional description
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Result of creating a PaymentIntent
    /// </summary>
    public record CreatePaymentIntentResult
    {
        /// <summary>
        /// Whether the operation succeeded
        /// </summary>
        public required bool IsSuccess { get; init; }

        /// <summary>
        /// Client secret for Stripe Elements (frontend)
        /// </summary>
        public string? ClientSecret { get; init; }

        /// <summary>
        /// PaymentIntent ID
        /// </summary>
        public string? PaymentIntentId { get; init; }

        /// <summary>
        /// Amount in base currency units
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Currency code
        /// </summary>
        public string? Currency { get; init; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Error code if failed
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Metadata from Stripe
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        public static CreatePaymentIntentResult Success(
            string clientSecret,
            string paymentIntentId,
            decimal amount,
            string currency,
            Dictionary<string, string>? metadata = null)
        {
            return new CreatePaymentIntentResult
            {
                IsSuccess = true,
                ClientSecret = clientSecret,
                PaymentIntentId = paymentIntentId,
                Amount = amount,
                Currency = currency,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }

        public static CreatePaymentIntentResult Failure(
            string errorMessage,
            string? errorCode = null,
            Dictionary<string, string>? metadata = null)
        {
            return new CreatePaymentIntentResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }
    }
}
