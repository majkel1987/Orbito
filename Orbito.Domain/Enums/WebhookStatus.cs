namespace Orbito.Domain.Enums;

/// <summary>
/// Status of webhook processing
/// </summary>
public enum WebhookStatus
{
    /// <summary>
    /// Webhook received but not yet processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Webhook successfully processed
    /// </summary>
    Processed = 1,

    /// <summary>
    /// Webhook processing failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Webhook is currently being processed
    /// </summary>
    Processing = 3
}
