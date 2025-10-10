namespace Orbito.Application.Common.Constants;

/// <summary>
/// Constants for validation rules
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Maximum date range for metrics queries (in days)
    /// </summary>
    public const int MaxDateRangeDays = 365;

    /// <summary>
    /// Minimum date range for metrics queries (in days)
    /// </summary>
    public const int MinDateRangeDays = 1;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum number of payment attempts per time window
    /// </summary>
    public const int MaxPaymentAttemptsPerWindow = 10;

    /// <summary>
    /// Payment attempt time window (in minutes)
    /// </summary>
    public const int PaymentAttemptWindowMinutes = 60;

    /// <summary>
    /// Maximum webhook payload size (in bytes)
    /// </summary>
    public const int MaxWebhookPayloadSize = 1024 * 1024; // 1MB

    /// <summary>
    /// Maximum signature tolerance for webhooks (in seconds)
    /// </summary>
    public const int MaxSignatureToleranceSeconds = 600; // 10 minutes

    /// <summary>
    /// Default signature tolerance for webhooks (in seconds)
    /// </summary>
    public const int DefaultSignatureToleranceSeconds = 300; // 5 minutes
}
