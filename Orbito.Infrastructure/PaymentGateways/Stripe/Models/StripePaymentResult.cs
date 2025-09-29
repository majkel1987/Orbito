using Orbito.Domain.Enums;

namespace Orbito.Infrastructure.PaymentGateways.Stripe.Models
{
    /// <summary>
    /// Wynik płatności Stripe z dodatkowymi informacjami specyficznymi dla Stripe
    /// </summary>
    public class StripePaymentResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Status płatności
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// ID płatności w Stripe (pi_xxx)
        /// </summary>
        public string? StripePaymentIntentId { get; set; }

        /// <summary>
        /// ID klienta w Stripe (cus_xxx)
        /// </summary>
        public string? StripeCustomerId { get; set; }

        /// <summary>
        /// ID metody płatności w Stripe (pm_xxx)
        /// </summary>
        public string? StripePaymentMethodId { get; set; }

        /// <summary>
        /// ID sesji Stripe Checkout (cs_xxx)
        /// </summary>
        public string? StripeSessionId { get; set; }

        /// <summary>
        /// Komunikat błędu (jeśli wystąpił)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Kod błędu Stripe (jeśli wystąpił)
        /// </summary>
        public string? StripeErrorCode { get; set; }

        /// <summary>
        /// Typ błędu Stripe (jeśli wystąpił)
        /// </summary>
        public string? StripeErrorType { get; set; }

        /// <summary>
        /// URL do przekierowania (dla Stripe Checkout)
        /// </summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>
        /// Metadane odpowiedzi
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Data utworzenia płatności w Stripe
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Data zakończenia płatności w Stripe
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Metoda płatności użyta w Stripe
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Ostatnie 4 cyfry karty
        /// </summary>
        public string? LastFourDigits { get; set; }

        /// <summary>
        /// Marka karty
        /// </summary>
        public string? CardBrand { get; set; }

        /// <summary>
        /// Kraj karty
        /// </summary>
        public string? CardCountry { get; set; }

        /// <summary>
        /// Typ karty (debit/credit)
        /// </summary>
        public string? CardType { get; set; }

        /// <summary>
        /// Czy karta jest 3D Secure
        /// </summary>
        public bool IsThreeDSecure { get; set; }

        /// <summary>
        /// Status 3D Secure
        /// </summary>
        public string? ThreeDSecureStatus { get; set; }

        /// <summary>
        /// Konstruktor dla sukcesu
        /// </summary>
        public static StripePaymentResult Success(
            PaymentStatus status,
            string stripePaymentIntentId,
            string? stripeCustomerId = null,
            string? stripePaymentMethodId = null,
            string? checkoutUrl = null,
            Dictionary<string, string>? metadata = null)
        {
            return new StripePaymentResult
            {
                IsSuccess = true,
                Status = status,
                StripePaymentIntentId = stripePaymentIntentId,
                StripeCustomerId = stripeCustomerId,
                StripePaymentMethodId = stripePaymentMethodId,
                CheckoutUrl = checkoutUrl,
                Metadata = metadata ?? new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Konstruktor dla błędu
        /// </summary>
        public static StripePaymentResult Failure(
            string errorMessage,
            string? stripeErrorCode = null,
            string? stripeErrorType = null,
            Dictionary<string, string>? metadata = null)
        {
            return new StripePaymentResult
            {
                IsSuccess = false,
                Status = PaymentStatus.Failed,
                ErrorMessage = errorMessage,
                StripeErrorCode = stripeErrorCode,
                StripeErrorType = stripeErrorType,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }
    }
}
