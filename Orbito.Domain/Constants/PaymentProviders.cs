namespace Orbito.Domain.Constants;

/// <summary>
/// String constants for payment providers.
/// Use PaymentProvider enum when possible; these strings are for external API integration.
/// </summary>
public static class PaymentProviders
{
    /// <summary>
    /// Stripe payment gateway identifier
    /// </summary>
    public const string Stripe = "Stripe";

    /// <summary>
    /// PayPal payment gateway identifier
    /// </summary>
    public const string PayPal = "PayPal";

    /// <summary>
    /// Square payment gateway identifier
    /// </summary>
    public const string Square = "Square";
}
