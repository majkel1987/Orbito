namespace Orbito.Infrastructure.PaymentGateways.Stripe
{
    /// <summary>
    /// Konfiguracja Stripe payment gateway
    /// </summary>
    public class StripeConfiguration
    {
        /// <summary>
        /// Klucz API Stripe (Secret Key)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Klucz publiczny Stripe (Publishable Key)
        /// </summary>
        public string PublishableKey { get; set; } = string.Empty;

        /// <summary>
        /// Webhook secret dla walidacji webhooków
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;

        /// <summary>
        /// Środowisko Stripe (test/live)
        /// </summary>
        public string Environment { get; set; } = "test";

        /// <summary>
        /// Waliduje konfigurację Stripe
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SecretKey) && 
                   !string.IsNullOrWhiteSpace(PublishableKey);
        }

        /// <summary>
        /// Sprawdza czy konfiguracja jest dla środowiska testowego
        /// </summary>
        public bool IsTestEnvironment()
        {
            return Environment.Equals("test", StringComparison.OrdinalIgnoreCase) ||
                   SecretKey.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sprawdza czy konfiguracja jest dla środowiska produkcyjnego
        /// </summary>
        public bool IsLiveEnvironment()
        {
            return Environment.Equals("live", StringComparison.OrdinalIgnoreCase) ||
                   SecretKey.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sprawdza czy konfiguracja webhook secret jest prawidłowa
        /// </summary>
        public bool HasValidWebhookSecret()
        {
            return !string.IsNullOrWhiteSpace(WebhookSecret) &&
                   WebhookSecret.StartsWith("whsec_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sprawdza czy klucze API są prawidłowe dla środowiska
        /// </summary>
        public bool HasValidKeysForEnvironment()
        {
            if (IsTestEnvironment())
            {
                return SecretKey.StartsWith("sk_test_", StringComparison.OrdinalIgnoreCase) &&
                       PublishableKey.StartsWith("pk_test_", StringComparison.OrdinalIgnoreCase);
            }

            if (IsLiveEnvironment())
            {
                return SecretKey.StartsWith("sk_live_", StringComparison.OrdinalIgnoreCase) &&
                       PublishableKey.StartsWith("pk_live_", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Waliduje pełną konfigurację Stripe
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateConfiguration()
        {
            if (!IsValid())
            {
                return (false, "Secret key and publishable key are required");
            }

            if (!HasValidKeysForEnvironment())
            {
                return (false, $"API keys don't match the configured environment ({Environment})");
            }

            return (true, string.Empty);
        }
    }
}
