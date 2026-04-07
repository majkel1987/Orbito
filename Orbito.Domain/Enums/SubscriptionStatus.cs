namespace Orbito.Domain.Enums;

/// <summary>
/// Status of a client subscription
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is active and in good standing
    /// </summary>
    Active = 1,

    /// <summary>
    /// Subscription was cancelled by user or provider
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Payment is past due
    /// </summary>
    PastDue = 3,

    /// <summary>
    /// Subscription is suspended (e.g., due to payment failure)
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Subscription is pending activation
    /// </summary>
    Pending = 5,

    /// <summary>
    /// Subscription has expired
    /// </summary>
    Expired = 6
}
