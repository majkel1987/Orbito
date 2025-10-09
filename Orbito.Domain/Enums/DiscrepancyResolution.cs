namespace Orbito.Domain.Enums;

/// <summary>
/// Resolution status of a payment discrepancy
/// </summary>
public enum DiscrepancyResolution
{
    /// <summary>
    /// Discrepancy is pending resolution
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Discrepancy was automatically resolved
    /// </summary>
    AutoResolved = 2,

    /// <summary>
    /// Discrepancy requires manual review
    /// </summary>
    RequiresManualReview = 3,

    /// <summary>
    /// Discrepancy was manually resolved
    /// </summary>
    ManuallyResolved = 4,

    /// <summary>
    /// Discrepancy was marked as false positive/ignored
    /// </summary>
    Ignored = 5
}
