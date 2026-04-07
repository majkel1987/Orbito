namespace Orbito.Application.DTOs
{
    public record PaymentDto
    {
        public required Guid Id { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SubscriptionId { get; init; }
        public required Guid ClientId { get; init; }
        public required decimal Amount { get; init; }
        public required string Currency { get; init; }
        public required string Status { get; init; }
        public string? ExternalTransactionId { get; init; }
        public string? PaymentMethod { get; init; }
        public string? ExternalPaymentId { get; init; }
        public string? PaymentMethodId { get; init; }
        public required DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; init; }
        public DateTime? FailedAt { get; init; }
        public DateTime? RefundedAt { get; init; }
        public string? FailureReason { get; init; }
    }
}
