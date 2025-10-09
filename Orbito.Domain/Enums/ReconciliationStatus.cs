namespace Orbito.Domain.Enums;

/// <summary>
/// Status of a reconciliation report
/// </summary>
public enum ReconciliationStatus
{
    /// <summary>
    /// Reconciliation is in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Reconciliation completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Reconciliation failed with errors
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Reconciliation completed with discrepancies found
    /// </summary>
    CompletedWithDiscrepancies = 4
}
