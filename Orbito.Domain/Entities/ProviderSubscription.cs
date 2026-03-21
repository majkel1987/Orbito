using Orbito.Domain.Common;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;

namespace Orbito.Domain.Entities;

/// <summary>
/// Subskrypcja platformowa Providera (Provider płaci Orbito).
/// Oddzielone od Subscription (subskrypcje klientów Providera).
/// </summary>
public class ProviderSubscription
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid PlatformPlanId { get; private set; }
    public ProviderSubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime TrialEndDate { get; private set; }
    public DateTime? PaidUntil { get; private set; }
    public DateTime? LastNotificationSentAt { get; private set; }
    public TrialNotificationTier LastNotificationTier { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public Provider Provider { get; private set; } = null!;
    public PlatformPlan PlatformPlan { get; private set; } = null!;

    // Computed Properties
    public bool IsTrialActive => Status == ProviderSubscriptionStatus.Trial
                                 && DateTime.UtcNow <= TrialEndDate;

    public int DaysRemaining
    {
        get
        {
            if (Status == ProviderSubscriptionStatus.Trial)
            {
                var remaining = (TrialEndDate - DateTime.UtcNow).TotalDays;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }
            if (Status == ProviderSubscriptionStatus.Active && PaidUntil.HasValue)
            {
                var remaining = (PaidUntil.Value - DateTime.UtcNow).TotalDays;
                return remaining > 0 ? (int)Math.Ceiling(remaining) : 0;
            }
            return 0;
        }
    }

    public bool IsExpired => Status == ProviderSubscriptionStatus.Expired
                             || (Status == ProviderSubscriptionStatus.Trial && DateTime.UtcNow > TrialEndDate)
                             || (Status == ProviderSubscriptionStatus.Active && PaidUntil.HasValue && DateTime.UtcNow > PaidUntil.Value);

    private ProviderSubscription() { } // EF Core

    public static ProviderSubscription CreateTrial(Guid providerId, Guid planId, int trialDays)
    {
        return new ProviderSubscription
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            PlatformPlanId = planId,
            Status = ProviderSubscriptionStatus.Trial,
            StartDate = DateTime.UtcNow,
            TrialEndDate = DateTime.UtcNow.AddDays(trialDays),
            LastNotificationTier = TrialNotificationTier.None,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Result Activate(DateTime paidUntil)
    {
        Status = ProviderSubscriptionStatus.Active;
        PaidUntil = paidUntil;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Expire()
    {
        if (Status == ProviderSubscriptionStatus.Active && PaidUntil > DateTime.UtcNow)
            return Result.Failure(DomainErrors.ProviderSubscription.StillActive);

        Status = ProviderSubscriptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == ProviderSubscriptionStatus.Cancelled)
            return Result.Failure(DomainErrors.ProviderSubscription.AlreadyCancelled);

        Status = ProviderSubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkNotificationSent(TrialNotificationTier tier)
    {
        if (tier <= LastNotificationTier)
            return Result.Failure(DomainErrors.ProviderSubscription.NotificationAlreadySent);

        LastNotificationTier = tier;
        LastNotificationSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
