using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class PaymentDiscrepancy
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ReconciliationReportId { get; set; }

    public Guid? PaymentId { get; set; }

    public string? ExternalPaymentId { get; set; }

    public int Type { get; set; }

    public int Resolution { get; set; }

    public int? OrbitoStatus { get; set; }

    public string? StripeStatus { get; set; }

    public decimal? OrbitoAmount { get; set; }

    public string? OrbitoCurrency { get; set; }

    public decimal? StripeAmount { get; set; }

    public string? StripeCurrency { get; set; }

    public string? ResolutionNotes { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedBy { get; set; }

    public string? AdditionalData { get; set; }

    public DateTime DetectedAt { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual ReconciliationReport ReconciliationReport { get; set; } = null!;
}
