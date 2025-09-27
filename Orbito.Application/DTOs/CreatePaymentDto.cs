namespace Orbito.Application.DTOs
{
    public record CreatePaymentDto
    {
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? ExternalTransactionId { get; init; }
        public string? PaymentMethod { get; init; }
        public string? ExternalPaymentId { get; init; }
    }
}
