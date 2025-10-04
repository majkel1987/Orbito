namespace Orbito.Application.Common.Configuration;

/// <summary>
/// Configuration constants for payment processing
/// </summary>
public static class PaymentProcessingConfiguration
{
    /// <summary>
    /// Time window to check for duplicate payment attempts (race condition prevention)
    /// </summary>
    public static readonly TimeSpan DuplicatePaymentCheckWindow = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Minimum age of pending payment before retry processing
    /// </summary>
    public static readonly TimeSpan PendingPaymentMinAge = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum time before payment times out
    /// </summary>
    public static readonly TimeSpan PaymentTimeout = TimeSpan.FromHours(24);

    /// <summary>
    /// Default payment method (should be from configuration in production)
    /// </summary>
    public static readonly string DefaultPaymentMethod = "Stripe";

    /// <summary>
    /// Default currency when plan is not found (fallback)
    /// </summary>
    public static readonly string DefaultCurrency = "USD";
}
