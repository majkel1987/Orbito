using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.Payments.Commands.CreatePaymentIntent
{
    /// <summary>
    /// Command to create a Stripe PaymentIntent for client portal payments.
    /// PCI DSS compliant - card data never touches our servers.
    /// </summary>
    public record CreatePaymentIntentCommand : IRequest<Result<CreatePaymentIntentResponse>>
    {
        /// <summary>
        /// The subscription to pay for
        /// </summary>
        public Guid SubscriptionId { get; init; }
    }

    /// <summary>
    /// Response containing the client secret for Stripe Elements
    /// </summary>
    public record CreatePaymentIntentResponse
    {
        /// <summary>
        /// Client secret for Stripe.js confirmPayment()
        /// </summary>
        public required string ClientSecret { get; init; }

        /// <summary>
        /// PaymentIntent ID for tracking
        /// </summary>
        public required string PaymentIntentId { get; init; }

        /// <summary>
        /// Amount in base currency units (e.g., PLN, not grosze)
        /// </summary>
        public required decimal Amount { get; init; }

        /// <summary>
        /// Currency code (e.g., "PLN", "USD")
        /// </summary>
        public required string Currency { get; init; }
    }
}
