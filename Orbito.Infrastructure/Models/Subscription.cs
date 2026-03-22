using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class Subscription
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ClientId { get; set; }

    public Guid PlanId { get; set; }

    public string Status { get; set; } = null!;

    public decimal CurrentPriceAmount { get; set; }

    public string CurrentPriceCurrency { get; set; } = null!;

    public int BillingPeriodValue { get; set; }

    public string BillingPeriodType { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime NextBillingDate { get; set; }

    public string? ExternalSubscriptionId { get; set; }

    public bool IsInTrial { get; set; }

    public DateTime? TrialEndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual Provider Tenant { get; set; } = null!;
}
