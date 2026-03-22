using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class ReconciliationReport
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTime RunDate { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int Status { get; set; }

    public int TotalPayments { get; set; }

    public int MatchedPayments { get; set; }

    public int MismatchedPayments { get; set; }

    public int DiscrepanciesCount { get; set; }

    public int AutoResolvedCount { get; set; }

    public int ManualReviewCount { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeOnly? Duration { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual ICollection<PaymentDiscrepancy> PaymentDiscrepancies { get; set; } = new List<PaymentDiscrepancy>();
}
