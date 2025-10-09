using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Represents a discrepancy found during payment reconciliation
/// </summary>
public class PaymentDiscrepancy : IMustHaveTenant
{
    public Guid Id { get; set; }
    public TenantId TenantId { get; set; }

    // Reconciliation report reference
    public Guid ReconciliationReportId { get; set; }

    // Payment reference
    public Guid? PaymentId { get; set; }
    public string? ExternalPaymentId { get; set; }

    // Discrepancy details
    public DiscrepancyType Type { get; set; }
    public DiscrepancyResolution Resolution { get; set; }

    // Status comparison
    public PaymentStatus? OrbitoStatus { get; set; }
    public string? StripeStatus { get; set; }

    // Amount comparison
    public decimal? OrbitoAmount { get; set; }
    public string? OrbitoCurrency { get; set; }
    public decimal? StripeAmount { get; set; }
    public string? StripeCurrency { get; set; }

    // Resolution details
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }

    // Additional metadata
    public string? AdditionalData { get; set; }
    public DateTime DetectedAt { get; set; }

    // Navigation properties
    public ReconciliationReport ReconciliationReport { get; set; } = null!;
    public Payment? Payment { get; set; }

    private PaymentDiscrepancy() { }

    /// <summary>
    /// Creates a status mismatch discrepancy
    /// </summary>
    public static PaymentDiscrepancy CreateStatusMismatch(
        TenantId tenantId,
        Guid reconciliationReportId,
        Guid paymentId,
        PaymentStatus orbitoStatus,
        string stripeStatus,
        string? externalPaymentId = null)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (string.IsNullOrWhiteSpace(stripeStatus))
            throw new ArgumentException("Stripe status cannot be empty", nameof(stripeStatus));

        return new PaymentDiscrepancy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReconciliationReportId = reconciliationReportId,
            PaymentId = paymentId,
            ExternalPaymentId = externalPaymentId,
            Type = DiscrepancyType.StatusMismatch,
            Resolution = DiscrepancyResolution.Pending,
            OrbitoStatus = orbitoStatus,
            StripeStatus = stripeStatus,
            DetectedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an amount mismatch discrepancy
    /// </summary>
    public static PaymentDiscrepancy CreateAmountMismatch(
        TenantId tenantId,
        Guid reconciliationReportId,
        Guid paymentId,
        decimal orbitoAmount,
        string orbitoCurrency,
        decimal stripeAmount,
        string stripeCurrency,
        string? externalPaymentId = null)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (string.IsNullOrWhiteSpace(orbitoCurrency))
            throw new ArgumentException("Orbito currency cannot be empty", nameof(orbitoCurrency));

        if (string.IsNullOrWhiteSpace(stripeCurrency))
            throw new ArgumentException("Stripe currency cannot be empty", nameof(stripeCurrency));

        return new PaymentDiscrepancy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReconciliationReportId = reconciliationReportId,
            PaymentId = paymentId,
            ExternalPaymentId = externalPaymentId,
            Type = DiscrepancyType.AmountMismatch,
            Resolution = DiscrepancyResolution.RequiresManualReview,
            OrbitoAmount = orbitoAmount,
            OrbitoCurrency = orbitoCurrency,
            StripeAmount = stripeAmount,
            StripeCurrency = stripeCurrency,
            DetectedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a missing payment discrepancy
    /// </summary>
    public static PaymentDiscrepancy CreateMissingPayment(
        TenantId tenantId,
        Guid reconciliationReportId,
        DiscrepancyType type,
        Guid? paymentId = null,
        string? externalPaymentId = null,
        string? additionalData = null)
    {
        if (tenantId == null)
            throw new ArgumentNullException(nameof(tenantId));

        if (type is not (DiscrepancyType.MissingInStripe or DiscrepancyType.MissingInOrbito))
            throw new ArgumentException("Invalid discrepancy type for missing payment", nameof(type));

        if (paymentId == null && string.IsNullOrWhiteSpace(externalPaymentId))
            throw new ArgumentException("Either PaymentId or ExternalPaymentId must be provided");

        return new PaymentDiscrepancy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReconciliationReportId = reconciliationReportId,
            PaymentId = paymentId,
            ExternalPaymentId = externalPaymentId,
            Type = type,
            Resolution = type == DiscrepancyType.MissingInStripe
                ? DiscrepancyResolution.RequiresManualReview
                : DiscrepancyResolution.Pending,
            AdditionalData = additionalData,
            DetectedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the discrepancy as auto-resolved
    /// </summary>
    public void MarkAsAutoResolved(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Resolution notes cannot be empty", nameof(notes));

        // NULL SAFETY: Ensure we don't overwrite existing resolution
        if (Resolution != DiscrepancyResolution.Pending)
        {
            throw new InvalidOperationException($"Cannot auto-resolve discrepancy that is already {Resolution}");
        }

        Resolution = DiscrepancyResolution.AutoResolved;
        ResolutionNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = "System";
    }

    /// <summary>
    /// Marks the discrepancy as requiring manual review
    /// </summary>
    public void MarkAsRequiresManualReview(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty", nameof(reason));

        Resolution = DiscrepancyResolution.RequiresManualReview;
        ResolutionNotes = reason;
    }

    /// <summary>
    /// Marks the discrepancy as manually resolved
    /// </summary>
    public void MarkAsManuallyResolved(string notes, string resolvedBy)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Resolution notes cannot be empty", nameof(notes));

        if (string.IsNullOrWhiteSpace(resolvedBy))
            throw new ArgumentException("Resolved by cannot be empty", nameof(resolvedBy));

        Resolution = DiscrepancyResolution.ManuallyResolved;
        ResolutionNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
    }

    /// <summary>
    /// Marks the discrepancy as ignored
    /// </summary>
    public void MarkAsIgnored(string reason, string ignoredBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty", nameof(reason));

        if (string.IsNullOrWhiteSpace(ignoredBy))
            throw new ArgumentException("Ignored by cannot be empty", nameof(ignoredBy));

        Resolution = DiscrepancyResolution.Ignored;
        ResolutionNotes = reason;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = ignoredBy;
    }
}
