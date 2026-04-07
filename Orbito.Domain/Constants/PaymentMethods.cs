namespace Orbito.Domain.Constants;

/// <summary>
/// String constants for payment methods.
/// Use PaymentMethodType enum for type-safe operations; these strings are for external API integration.
/// </summary>
public static class PaymentMethods
{
    /// <summary>
    /// Credit or debit card payment
    /// </summary>
    public const string Card = "Card";

    /// <summary>
    /// Bank transfer payment
    /// </summary>
    public const string BankTransfer = "BankTransfer";

    /// <summary>
    /// BLIK (Polish instant payment)
    /// </summary>
    public const string Blik = "Blik";

    /// <summary>
    /// PayPal payment
    /// </summary>
    public const string PayPal = "PayPal";

    /// <summary>
    /// Stripe payment (used for direct Stripe integration)
    /// </summary>
    public const string Stripe = "Stripe";

    /// <summary>
    /// Cash payment
    /// </summary>
    public const string Cash = "Cash";

    /// <summary>
    /// Set of all valid payment methods for validation
    /// </summary>
    public static readonly HashSet<string> ValidPaymentMethods = new()
    {
        Card,
        BankTransfer,
        Blik,
        PayPal,
        Stripe,
        Cash
    };

    /// <summary>
    /// Validates if the payment method string is recognized
    /// </summary>
    /// <param name="paymentMethod">Payment method to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string? paymentMethod)
    {
        return !string.IsNullOrEmpty(paymentMethod) &&
               ValidPaymentMethods.Contains(paymentMethod);
    }
}