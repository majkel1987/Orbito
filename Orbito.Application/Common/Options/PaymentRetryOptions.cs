namespace Orbito.Application.Common.Options
{
    /// <summary>
    /// Configuration options for payment retry logic
    /// </summary>
    public class PaymentRetryOptions
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
        public const string SectionName = "PaymentRetry";

        /// <summary>
        /// Retry delays using exponential backoff
        /// Default: [5m, 15m, 1h, 6h, 24h]
        /// </summary>
        public TimeSpan[] RetryDelays { get; set; } = new[]
        {
            TimeSpan.FromMinutes(5),   // 1st retry: 5 minutes
            TimeSpan.FromMinutes(15),  // 2nd retry: 15 minutes
            TimeSpan.FromHours(1),     // 3rd retry: 1 hour
            TimeSpan.FromHours(6),     // 4th retry: 6 hours
            TimeSpan.FromHours(24)     // 5th retry: 24 hours
        };

        /// <summary>
        /// Timeout for stuck InProgress retries (recovery mechanism)
        /// Default: 30 minutes
        /// </summary>
        public TimeSpan InProgressTimeout { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Maximum number of concurrent retry processing tasks
        /// Default: 10
        /// </summary>
        public int MaxConcurrency { get; set; } = 10;

        /// <summary>
        /// Maximum number of retry attempts
        /// Default: 5
        /// </summary>
        public int MaxAttempts { get; set; } = 5;

        /// <summary>
        /// Enable actual payment processing (Stripe/gateway integration)
        /// Set to false in development/testing to simulate retries without real payment processing
        /// Default: false (safe for development)
        /// </summary>
        public bool EnablePaymentProcessing { get; set; } = false;
    }
}
