namespace Orbito.Application.Common.Configuration;

/// <summary>
/// Configuration settings for payment reconciliation
/// </summary>
public class ReconciliationSettings
{
    /// <summary>
    /// Maximum reconciliation period in days
    /// </summary>
    public int MaxReconciliationPeriodDays { get; set; } = 30;

    /// <summary>
    /// Maximum historical data reconciliation period in years
    /// </summary>
    public int MaxHistoricalDataYears { get; set; } = 1;

    /// <summary>
    /// Stripe API batch size for fetching payments
    /// </summary>
    public int StripeBatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum parallel tasks for processing
    /// </summary>
    public int MaxParallelTasks { get; set; } = 5;

    /// <summary>
    /// Rate limiting delay in milliseconds for Stripe API
    /// </summary>
    public int StripeApiDelayMs { get; set; } = 100;

    /// <summary>
    /// Number of requests before applying rate limit delay
    /// </summary>
    public int RateLimitBatchSize { get; set; } = 10;

    /// <summary>
    /// Daily reconciliation run time (HH:mm format)
    /// </summary>
    public string DailyRunTime { get; set; } = "02:00";

    /// <summary>
    /// Email recipients for reconciliation reports
    /// </summary>
    public List<string> EmailRecipients { get; set; } = new();

    /// <summary>
    /// Slack webhook URL for critical notifications
    /// </summary>
    public string? SlackWebhookUrl { get; set; }

    /// <summary>
    /// Threshold for critical discrepancy count
    /// </summary>
    public int CriticalDiscrepancyThreshold { get; set; } = 10;

    /// <summary>
    /// Whether to enable auto-resolution of discrepancies
    /// </summary>
    public bool EnableAutoResolution { get; set; } = true;

    /// <summary>
    /// Timeout for reconciliation operations in minutes
    /// </summary>
    public int OperationTimeoutMinutes { get; set; } = 10;
}
