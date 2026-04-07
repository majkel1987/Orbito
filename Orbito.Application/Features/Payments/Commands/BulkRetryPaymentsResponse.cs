namespace Orbito.Application.Features.Payments.Commands;

/// <summary>
/// Response for bulk retry payments command
/// </summary>
public record BulkRetryPaymentsResponse
{
    /// <summary>
    /// Total number of payments processed
    /// </summary>
    public required int TotalProcessed { get; init; }

    /// <summary>
    /// Number of successful retry schedules created
    /// </summary>
    public required int SuccessfulRetries { get; init; }

    /// <summary>
    /// Number of failed retry attempts
    /// </summary>
    public required int FailedRetries { get; init; }

    /// <summary>
    /// List of individual retry results
    /// </summary>
    public required List<BulkRetryItemResult> Results { get; init; }
}

/// <summary>
/// Result for individual payment retry in bulk operation
/// </summary>
public record BulkRetryItemResult
{
    /// <summary>
    /// ID of the payment
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// Whether the retry was scheduled successfully
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// ID of the created retry schedule
    /// </summary>
    public Guid? RetryScheduleId { get; init; }

    /// <summary>
    /// Error message if retry failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// When the next retry attempt will be made
    /// </summary>
    public DateTime? NextAttemptAt { get; init; }

    /// <summary>
    /// Current attempt number
    /// </summary>
    public int AttemptNumber { get; init; }
}

