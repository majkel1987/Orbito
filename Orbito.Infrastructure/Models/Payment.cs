using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SubscriptionId { get; set; }

    public Guid ClientId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? ExternalTransactionId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? ExternalPaymentId { get; set; }

    public string? PaymentMethodId { get; set; }

    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public string? FailureReason { get; set; }

    public string? RefundReason { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<PaymentDiscrepancy> PaymentDiscrepancies { get; set; } = new List<PaymentDiscrepancy>();

    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();

    public virtual PaymentRetrySchedule? PaymentRetrySchedule { get; set; }

    public virtual Subscription Subscription { get; set; } = null!;
}
