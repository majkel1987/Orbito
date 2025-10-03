namespace Orbito.Domain.Enums
{
    /// <summary>
    /// Payment method types
    /// </summary>
    public enum PaymentMethodType
    {
        /// <summary>
        /// Credit or debit card
        /// </summary>
        Card = 1,

        /// <summary>
        /// Bank transfer
        /// </summary>
        BankTransfer = 2,

        /// <summary>
        /// PayPal
        /// </summary>
        PayPal = 3,

        /// <summary>
        /// Stripe payment method
        /// </summary>
        Stripe = 4,

        /// <summary>
        /// Apple Pay
        /// </summary>
        ApplePay = 5,

        /// <summary>
        /// Google Pay
        /// </summary>
        GooglePay = 6,

        /// <summary>
        /// SEPA direct debit
        /// </summary>
        SEPA = 7,

        /// <summary>
        /// ACH transfer
        /// </summary>
        ACH = 8,

        /// <summary>
        /// Cryptocurrency
        /// </summary>
        Cryptocurrency = 9,

        /// <summary>
        /// Other payment method
        /// </summary>
        Other = 99
    }
}