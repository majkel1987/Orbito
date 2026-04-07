using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class SubscriptionPlan : IMustHaveTenant
    {
        public Guid Id { get; private set; }
        public TenantId TenantId { get; private set; }

        // Plan Details
        public string Name { get; private set; } = string.Empty;  // "Plan Business"
        public string? Description { get; private set; } // "5 artykułów + 3 rewizje"
        public Money Price { get; private set; }
        public BillingPeriod BillingPeriod { get; private set; }
        public int TrialDays { get; private set; }

        // Plan Features and Limitations (JSON)
        public string? FeaturesJson { get; private set; }      // Serialized features list
        public string? LimitationsJson { get; private set; }   // Serialized limitations list

        // Plan Settings
        public int TrialPeriodDays { get; private set; }       // Trial period in days
        public bool IsActive { get; private set; }
        public bool IsPublic { get; private set; }             // Visible on public page
        public int SortOrder { get; private set; }             // Display order
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation Properties
        public Provider Provider { get; private set; } = null!;
        private readonly List<Subscription> _subscriptions = [];
        public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

        private SubscriptionPlan() { } // EF Core

       public static SubscriptionPlan Create(
        TenantId tenantId,
        string name,
        decimal amount,
        string currency,
        BillingPeriodType billingPeriodType,
        string? description = null,
        int trialDays = 0,
        int trialPeriodDays = 0,
        string? featuresJson = null,
        string? limitationsJson = null,
        int sortOrder = 0)
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
            TrialPeriodDays = trialPeriodDays,
            FeaturesJson = featuresJson,
            LimitationsJson = limitationsJson,
            IsActive = true,
            IsPublic = true,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateFeatures(string? featuresJson)
        {
            FeaturesJson = featuresJson;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLimitations(string? limitationsJson)
        {
            LimitationsJson = limitationsJson;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTrialPeriod(int trialPeriodDays)
        {
            TrialPeriodDays = trialPeriodDays;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateVisibility(bool isPublic)
        {
            IsPublic = isPublic;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanBeDeleted()
        {
            return !Subscriptions.Any(s => s.Status == SubscriptionStatus.Active);
        }

        public Result UpdateBasicInfo(
            string name,
            string? description,
            int trialDays,
            int trialPeriodDays,
            int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.SubscriptionPlan.NameRequired);

            Name = name;
            Description = description;
            TrialDays = trialDays;
            TrialPeriodDays = trialPeriodDays;
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result UpdateBillingPeriod(BillingPeriod billingPeriod)
        {
            if (billingPeriod == null)
                return Result.Failure(DomainErrors.SubscriptionPlan.InvalidBillingPeriod);

            BillingPeriod = billingPeriod;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
