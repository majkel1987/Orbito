namespace Orbito.Domain.Constants
{
    public static class PaymentMethods
    {
        public const string Card = "Card";
        public const string BankTransfer = "BankTransfer";
        public const string Blik = "Blik";
        public const string PayPal = "PayPal";
        public const string Stripe = "Stripe";
        public const string Cash = "Cash";

        public static readonly HashSet<string> ValidPaymentMethods = new()
        {
            Card,
            BankTransfer,
            Blik,
            PayPal,
            Stripe,
            Cash
        };

        public static bool IsValid(string? paymentMethod)
        {
            return !string.IsNullOrEmpty(paymentMethod) &&
                   ValidPaymentMethods.Contains(paymentMethod);
        }
    }
}