using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Represents a payment reconciliation report
/// </summary>
public class ReconciliationReport : IMustHaveTenant
{
    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }

    // Report metadata
    public DateTime RunDate { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public ReconciliationStatus Status { get; private set; }

    // Statistics
    public int TotalPayments { get; private set; }
    public int MatchedPayments { get; private set; }
    public int MismatchedPayments { get; private set; }
    public int DiscrepanciesCount { get; private set; }
    public int AutoResolvedCount { get; private set; }
    public int ManualReviewCount { get; private set; }

    // Execution details
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Navigation properties
    private readonly List<PaymentDiscrepancy> _discrepancies = [];
    public IReadOnlyCollection<PaymentDiscrepancy> Discrepancies => _discrepancies.AsReadOnly();

    private ReconciliationReport() { }

    /// <summary>
    /// Creates a new reconciliation report
    /// </summary>
    public static ReconciliationReport Create(
        TenantId tenantId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (periodStart >= periodEnd)
            throw new ArgumentException("Period start must be before period end", nameof(periodStart));

        if (periodEnd > DateTime.UtcNow)
            throw new ArgumentException("Period end cannot be in the future", nameof(periodEnd));

        return new ReconciliationReport
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RunDate = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = ReconciliationStatus.InProgress,
            TotalPayments = 0,
            MatchedPayments = 0,
            MismatchedPayments = 0,
            DiscrepanciesCount = 0,
            AutoResolvedCount = 0,
            ManualReviewCount = 0,
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the reconciliation as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status != ReconciliationStatus.InProgress)
            throw new InvalidOperationException("Only in-progress reconciliations can be marked as completed");

        CompletedAt = DateTime.UtcNow;
        Duration = CompletedAt - StartedAt;
        Status = DiscrepanciesCount > 0
            ? ReconciliationStatus.CompletedWithDiscrepancies
            : ReconciliationStatus.Completed;
    }

    /// <summary>
    /// Marks the reconciliation as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));

        CompletedAt = DateTime.UtcNow;
        Duration = CompletedAt - StartedAt;
        Status = ReconciliationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Sets the total payments count
    /// </summary>
    public void SetTotalPayments(int count)
    {
        TotalPayments = count;
    }

    /// <summary>
    /// Sets the matched payments count
    /// </summary>
    public void SetMatchedPayments(int count)
    {
        MatchedPayments = count;
    }

    /// <summary>
    /// Updates statistics based on discrepancies
    /// </summary>
    public void UpdateStatistics()
    {
        DiscrepanciesCount = Discrepancies.Count;
        AutoResolvedCount = Discrepancies.Count(d => d.Resolution == DiscrepancyResolution.AutoResolved);
        ManualReviewCount = Discrepancies.Count(d => d.Resolution == DiscrepancyResolution.RequiresManualReview);

        // NULL SAFETY: Only count discrepancies with valid PaymentId (excludes MissingInOrbito)
        MismatchedPayments = Discrepancies
            .Where(d => d.PaymentId.HasValue)
            .Select(d => d.PaymentId!.Value)
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Adds a discrepancy to the report
    /// </summary>
    public void AddDiscrepancy(PaymentDiscrepancy discrepancy)
    {
        if (discrepancy == null)
            throw new ArgumentNullException(nameof(discrepancy));

        if (Status != ReconciliationStatus.InProgress)
            throw new InvalidOperationException("Cannot add discrepancies to a completed reconciliation");

        _discrepancies.Add(discrepancy);
        UpdateStatistics();
    }
}
