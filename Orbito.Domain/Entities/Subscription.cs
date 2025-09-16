using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Subscription : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }  // Which provider this belongs to

        // Subscription Details
        public Guid ClientId { get; set; }
        public int PlanId { get; set; }
        public SubscriptionStatus Status { get; set; }

        // Billing Information
        public Money CurrentPrice { get; set; }        // Price at subscription time
        public BillingPeriod BillingPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }

        // Trial Information
        public bool IsInTrial { get; set; }
        public DateTime? TrialEndDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Client Client { get; set; }
        public SubscriptionPlan Plan { get; set; }
        public ICollection<Payment> Payments { get; set; } = [];

        private Subscription() { } // EF Core

        public static Subscription Create(
            TenantId tenantId,
            Guid clientId,
            int planId,
            Money price,
            BillingPeriod billingPeriod,
            int trialDays = 0)
        {
            var startDate = DateTime.UtcNow;
            var isInTrial = trialDays > 0;
            var trialEndDate = isInTrial ? startDate.AddDays(trialDays) : (DateTime?)null;
            var nextBillingDate = isInTrial
                ? trialEndDate!.Value
                : billingPeriod.GetNextBillingDate(startDate);

            return new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = clientId,
                PlanId = planId,
                Status = SubscriptionStatus.Active,
                CurrentPrice = price,
                BillingPeriod = billingPeriod,
                StartDate = startDate,
                NextBillingDate = nextBillingDate,
                IsInTrial = isInTrial,
                TrialEndDate = trialEndDate,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Cancel()
        {
            Status = SubscriptionStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePlan(int newPlanId, Money newPrice)
        {
            PlanId = newPlanId;
            CurrentPrice = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
