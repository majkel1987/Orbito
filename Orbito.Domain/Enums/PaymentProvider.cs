namespace Orbito.Domain.Enums;

/// <summary>
/// Payment gateway providers supported by the platform
/// </summary>
public enum PaymentProvider
{
    /// <summary>
    /// Stripe payment gateway
    /// </summary>
    Stripe = 1,

    /// <summary>
    /// PayPal payment gateway
    /// </summary>
    PayPal = 2,

    /// <summary>
    /// Square payment gateway
    /// </summary>
    Square = 3
}
