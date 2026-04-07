namespace Orbito.Application.Features.Payments.Queries.DTOs;

/// <summary>
/// DTO for failed payment information used in retry operations.
/// </summary>
public record FailedPaymentDto
{
    /// <summary>
    /// ID of the payment.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID of the client.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// ID of the subscription.
    /// </summary>
    public required Guid SubscriptionId { get; init; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Payment currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// When the payment was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the payment failed.
    /// </summary>
    public DateTime? FailedAt { get; init; }

    /// <summary>
    /// Reason for the failure.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// External transaction ID.
    /// </summary>
    public string? ExternalTransactionId { get; init; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public string? PaymentMethod { get; init; }

    /// <summary>
    /// Whether the payment has an active retry schedule.
    /// </summary>
    public required bool HasActiveRetry { get; init; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public required int RetryAttempts { get; init; }

    /// <summary>
    /// Whether the payment can be retried.
    /// </summary>
    public required bool CanRetry { get; init; }
}
