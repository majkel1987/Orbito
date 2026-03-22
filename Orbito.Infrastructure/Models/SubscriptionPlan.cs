using System;
using System.Collections.Generic;

namespace Orbito.Infrastructure.Models;

public partial class SubscriptionPlan
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal PriceAmount { get; set; }

    public string PriceCurrency { get; set; } = null!;

    public int BillingPeriodValue { get; set; }

    public string BillingPeriodType { get; set; } = null!;

    public int TrialDays { get; set; }

    public string? FeaturesJson { get; set; }

    public string? LimitationsJson { get; set; }

    public int TrialPeriodDays { get; set; }

    public bool IsActive { get; set; }

    public bool IsPublic { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual Provider Tenant { get; set; } = null!;
}
