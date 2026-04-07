namespace Orbito.Application.Common.Settings;

/// <summary>
/// Configuration settings for payment processing
/// </summary>
public class PaymentSettings
{
    /// <summary>
    /// Maximum number of days to retry failed payments
    /// </summary>
    public int PaymentRetryDays { get; set; } = 30;

    /// <summary>
    /// Number of years before payment method expires
    /// </summary>
    public int PaymentMethodExpiryYears { get; set; } = 2;

    /// <summary>
    /// Maximum payment amount allowed in USD
    /// </summary>
    public decimal MaxPaymentAmount { get; set; } = 1000000;

    /// <summary>
    /// Minimum payment amount allowed in USD
    /// </summary>
    public decimal MinPaymentAmount { get; set; } = 0.01m;

    /// <summary>
    /// Minimum length for payment tokens
    /// </summary>
    public int TokenMinLength { get; set; } = 10;

    /// <summary>
    /// Maximum length for payment tokens
    /// </summary>
    public int TokenMaxLength { get; set; } = 500;
}
