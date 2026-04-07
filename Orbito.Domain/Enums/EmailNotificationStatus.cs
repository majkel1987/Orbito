namespace Orbito.Domain.Enums;

/// <summary>
/// Status of an email notification in the outbox
/// </summary>
public enum EmailNotificationStatus
{
    /// <summary>
    /// Email is pending processing
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Email is being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Email was processed successfully
    /// </summary>
    Processed = 3,

    /// <summary>
    /// Email processing failed
    /// </summary>
    Failed = 4
}
