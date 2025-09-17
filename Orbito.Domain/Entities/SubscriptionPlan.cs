using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class SubscriptionPlan : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }

        // Plan Details
        public string Name { get; set; }        // "Plan Business"
        public string? Description { get; set; } // "5 artykułów + 3 rewizje"
        public Money Price { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public int TrialDays { get; set; }

        // Plan Features (JSON or separate entity)
        public string? FeaturesJson { get; set; }  // Serialized features list

        // Plan Settings
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }      // Visible on public page
        public int SortOrder { get; set; }      // Display order
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Provider Provider { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = [];

        private SubscriptionPlan() { } // EF Core

       public static SubscriptionPlan Create(
        TenantId tenantId,
        string name,
        decimal amount,
        string currency,
        BillingPeriodType billingPeriodType,
        string? description = null,
        int trialDays = 0)
    {
        return new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            Price = Money.Create(amount, currency),
            BillingPeriod = BillingPeriod.Create(1, billingPeriodType),
            TrialDays = trialDays,
            IsActive = true,
            IsPublic = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
