namespace Orbito.Application.Common.Configuration;

/// <summary>
/// Configuration settings for payment processing
/// </summary>
public class PaymentProcessingConfiguration
{
    /// <summary>
    /// Time window to check for duplicate payment attempts (race condition prevention)
    /// </summary>
    public TimeSpan DuplicatePaymentCheckWindow { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Minimum age of pending payment before retry processing
    /// </summary>
    public TimeSpan PendingPaymentMinAge { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum time before payment times out
    /// </summary>
    public TimeSpan PaymentTimeout { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Default payment provider (configurable at runtime)
    /// </summary>
    public string DefaultPaymentProvider { get; set; } = "Stripe";

    /// <summary>
    /// Default currency when plan is not found (fallback)
    /// </summary>
    public string DefaultCurrency { get; set; } = "USD";
}
