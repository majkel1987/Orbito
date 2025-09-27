namespace Orbito.Application.Common.Settings
{
    public class PaymentSettings
    {
        public int PaymentRetryDays { get; set; } = 30;
        public int PaymentMethodExpiryYears { get; set; } = 2;
        public decimal MaxPaymentAmount { get; set; } = 1000000;
        public decimal MinPaymentAmount { get; set; } = 0.01m;
        public int TokenMinLength { get; set; } = 10;
        public int TokenMaxLength { get; set; } = 500;
    }
}
