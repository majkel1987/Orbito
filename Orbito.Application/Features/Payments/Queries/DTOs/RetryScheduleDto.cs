namespace Orbito.Application.Features.Payments.Queries.DTOs;

/// <summary>
/// DTO for retry schedule information.
/// </summary>
public record RetryScheduleDto
{
    /// <summary>
    /// ID of the retry schedule.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID of the payment.
    /// </summary>
    public required Guid PaymentId { get; init; }

    /// <summary>
    /// ID of the client.
    /// </summary>
    public required Guid ClientId { get; init; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Payment currency.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// When the next retry attempt will be made.
    /// </summary>
    public required DateTime NextAttemptAt { get; init; }

    /// <summary>
    /// Current attempt number.
    /// </summary>
    public required int AttemptNumber { get; init; }

    /// <summary>
    /// Maximum number of attempts allowed.
    /// </summary>
    public required int MaxAttempts { get; init; }

    /// <summary>
    /// Current status of the retry.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Last error message.
    /// </summary>
    public string? LastError { get; init; }

    /// <summary>
    /// When the retry schedule was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the retry schedule was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Whether the retry is overdue (snapshot value at query time).
    /// </summary>
    public required bool IsOverdue { get; init; }

    /// <summary>
    /// Whether the retry can be attempted (snapshot value at query time).
    /// </summary>
    public required bool CanRetry { get; init; }
}
