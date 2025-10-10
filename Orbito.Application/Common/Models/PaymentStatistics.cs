namespace Orbito.Application.Common.Models;

/// <summary>
/// Comprehensive payment statistics for a specific period
/// </summary>
public record PaymentStatistics
{
    /// <summary>
    /// Total number of payments in the period
    /// </summary>
    public int TotalPayments { get; init; }

    /// <summary>
    /// Number of completed payments
    /// </summary>
    public int CompletedPayments { get; init; }

    /// <summary>
    /// Number of failed payments
    /// </summary>
    public int FailedPayments { get; init; }

    /// <summary>
    /// Number of pending payments
    /// </summary>
    public int PendingPayments { get; init; }

    /// <summary>
    /// Number of processing payments
    /// </summary>
    public int ProcessingPayments { get; init; }

    /// <summary>
    /// Number of refunded payments
    /// </summary>
    public int RefundedPayments { get; init; }

    /// <summary>
    /// Success rate percentage (0-100)
    /// </summary>
    public decimal SuccessRate { get; init; }

    /// <summary>
    /// Average processing time in seconds
    /// </summary>
    public decimal AverageProcessingTimeSeconds { get; init; }

    /// <summary>
    /// Total revenue from completed payments
    /// </summary>
    public decimal TotalRevenue { get; init; }

    /// <summary>
    /// Currency of the revenue
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// Breakdown of payments by status
    /// </summary>
    public Dictionary<string, int> PaymentsByStatus { get; init; } = new();

    /// <summary>
    /// Breakdown of payments by payment method
    /// </summary>
    public Dictionary<string, int> PaymentsByMethod { get; init; } = new();

    /// <summary>
    /// Breakdown of failure reasons
    /// </summary>
    public Dictionary<string, int> FailureReasons { get; init; } = new();

    /// <summary>
    /// Date range this statistics cover
    /// </summary>
    public DateRange Period { get; init; } = new();

    /// <summary>
    /// Provider ID these statistics belong to (null for all providers)
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// When these statistics were calculated
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}
