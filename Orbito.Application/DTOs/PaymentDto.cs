namespace Orbito.Application.DTOs
{
    public record PaymentDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid SubscriptionId { get; init; }
        public Guid ClientId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? ExternalTransactionId { get; init; }
        public string? PaymentMethod { get; init; }
        public string? ExternalPaymentId { get; init; }
        public string? PaymentMethodId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; init; }
        public DateTime? FailedAt { get; init; }
        public DateTime? RefundedAt { get; init; }
        public string? FailureReason { get; init; }
    }
}
