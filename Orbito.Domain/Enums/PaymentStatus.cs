namespace Orbito.Domain.Enums;

/// <summary>
/// Status of a payment transaction
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending processing
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Payment completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Payment was fully refunded
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// Payment was partially refunded
    /// </summary>
    PartiallyRefunded = 6,

    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled = 7
}

