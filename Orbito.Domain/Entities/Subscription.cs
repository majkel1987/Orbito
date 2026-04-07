using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class Subscription : IMustHaveTenant
    {
        public Guid Id { get; private set; }
        public TenantId TenantId { get; private set; }  // Which provider this belongs to

        // Subscription Details
        public Guid ClientId { get; private set; }
        public Guid PlanId { get; private set; }
        public SubscriptionStatus Status { get; private set; }

        // Billing Information
        public Money CurrentPrice { get; private set; }        // Price at subscription time
        public BillingPeriod BillingPeriod { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public DateTime NextBillingDate { get; private set; }

        // External Integration
        public string? ExternalSubscriptionId { get; private set; }  // External provider subscription ID (e.g., Stripe)

        // Trial Information
        public bool IsInTrial { get; private set; }
        public DateTime? TrialEndDate { get; private set; }

        // Timestamps
        public DateTime CreatedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation Properties
        public Client Client { get; private set; } = null!;
        public SubscriptionPlan Plan { get; private set; } = null!;
        private readonly List<Payment> _payments = [];
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        private Subscription() { } // EF Core

        public static Subscription Create(
            TenantId tenantId,
            Guid clientId,
            Guid planId,
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

        public void Activate()
        {
            if (Status == SubscriptionStatus.Pending || Status == SubscriptionStatus.Suspended)
            {
                Status = SubscriptionStatus.Active;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Suspend()
        {
            if (Status == SubscriptionStatus.Active)
            {
                Status = SubscriptionStatus.Suspended;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Resume()
        {
            if (Status == SubscriptionStatus.Suspended)
            {
                Status = SubscriptionStatus.Active;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void MarkAsPastDue()
        {
            if (Status == SubscriptionStatus.Active)
            {
                Status = SubscriptionStatus.PastDue;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void MarkAsExpired()
        {
            Status = SubscriptionStatus.Expired;
            EndDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result ChangePlan(Guid newPlanId, Money newPrice)
        {
            if (newPlanId == Guid.Empty)
                return Result.Failure(DomainErrors.Subscription.PlanNotFound);

            if (newPrice == null || newPrice.Amount <= 0)
                return Result.Failure(DomainErrors.SubscriptionPlan.InvalidPrice);

            PlanId = newPlanId;
            CurrentPrice = newPrice;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public void UpdateNextBillingDate()
        {
            NextBillingDate = BillingPeriod.GetNextBillingDate(DateTime.UtcNow);
            UpdatedAt = DateTime.UtcNow;
        }

        public void EndTrial()
        {
            if (IsInTrial)
            {
                IsInTrial = false;
                TrialEndDate = DateTime.UtcNow;
                NextBillingDate = BillingPeriod.GetNextBillingDate(DateTime.UtcNow);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public bool CanBeUpgraded()
        {
            return Status == SubscriptionStatus.Active && !IsInTrial;
        }

        public bool CanBeDowngraded()
        {
            return Status == SubscriptionStatus.Active && !IsInTrial;
        }

        public bool CanBeCancelled()
        {
            return Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Suspended;
        }

        public bool CanBeSuspended()
        {
            return Status == SubscriptionStatus.Active;
        }

        public bool CanBeResumed()
        {
            return Status == SubscriptionStatus.Suspended;
        }

        public bool CanBePaid()
        {
            return Status == SubscriptionStatus.Active || Status == SubscriptionStatus.PastDue;
        }

        public Result ProcessPayment(Guid paymentId)
        {
            if (!CanBePaid())
                return Result.Failure(DomainErrors.Subscription.NotActive);

            if (Status == SubscriptionStatus.PastDue)
                Status = SubscriptionStatus.Active;

            UpdateNextBillingDate();
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public bool IsExpiring(DateTime checkDate, int daysBeforeExpiration = 7)
        {
            return Status == SubscriptionStatus.Active && 
                   NextBillingDate <= checkDate.AddDays(daysBeforeExpiration) &&
                   NextBillingDate > checkDate;
        }

        public bool IsExpired(DateTime checkDate)
        {
            return Status == SubscriptionStatus.Active && NextBillingDate <= checkDate;
        }

        public void MarkAsUnpaid()
        {
            if (Status == SubscriptionStatus.Active || Status == SubscriptionStatus.PastDue)
            {
                Status = SubscriptionStatus.PastDue;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void StartTrial(DateTime trialEndDate)
        {
            IsInTrial = true;
            TrialEndDate = trialEndDate;
            NextBillingDate = trialEndDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsCanceled => Status == SubscriptionStatus.Cancelled;

        public void ScheduleCancellation(DateTime cancelDate)
        {
            EndDate = cancelDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            Status = SubscriptionStatus.Cancelled;
            EndDate = DateTime.UtcNow;
            CancelledAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Renew(DateTime nextBillingDate)
        {
            NextBillingDate = nextBillingDate;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
