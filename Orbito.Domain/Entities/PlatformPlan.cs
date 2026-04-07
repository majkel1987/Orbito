using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities;

/// <summary>
/// Plan platformowy Orbito (Starter, Pro, Enterprise).
/// Provider wybiera plan przy rejestracji i płaci za niego Orbito.
/// Oddzielone od SubscriptionPlan (plany, które Provider sprzedaje swoim klientom).
/// </summary>
public class PlatformPlan
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = Money.Zero("PLN");
    public BillingPeriod BillingPeriod { get; private set; } = BillingPeriod.Monthly();
    public int TrialDays { get; private set; } = 14;
    public bool IsActive { get; private set; } = true;
    public string? FeaturesJson { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation Properties
    private readonly List<ProviderSubscription> _providerSubscriptions = [];
    public IReadOnlyCollection<ProviderSubscription> ProviderSubscriptions => _providerSubscriptions.AsReadOnly();

    private PlatformPlan() { } // EF Core

    public static PlatformPlan Create(
        string name,
        Money price,
        int trialDays = 14,
        string? description = null,
        string? featuresJson = null,
        int sortOrder = 0)
    {
        return new PlatformPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            BillingPeriod = BillingPeriod.Monthly(),
            TrialDays = trialDays,
            IsActive = true,
            FeaturesJson = featuresJson,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
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
}
