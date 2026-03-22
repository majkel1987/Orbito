using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.CreateProviderPaymentIntent
{
    /// <summary>
    /// Command to create a Stripe PaymentIntent for Provider platform subscription.
    /// PCI DSS compliant - card data never touches our servers.
    /// </summary>
    public record CreateProviderPaymentIntentCommand : IRequest<Result<CreateProviderPaymentIntentResponse>>
    {
        /// <summary>
        /// Optional: PlatformPlanId to pay for (null = use current plan)
        /// </summary>
        public Guid? PlatformPlanId { get; init; }
    }

    /// <summary>
    /// Response containing the client secret for Stripe Elements
    /// </summary>
    public record CreateProviderPaymentIntentResponse
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
        /// Currency code (e.g., "PLN")
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Name of the plan being paid for
        /// </summary>
        public required string PlanName { get; init; }
    }
}
