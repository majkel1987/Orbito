using System.Reflection;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

public class SubscriptionPlanTestDataBuilder
{
    private Guid _id = Guid.NewGuid();
    private TenantId _tenantId = TenantId.New();
    private string _name = "Test Plan";
    private string? _description = "Test plan description";
    private Money _price = Money.Create(29.99m, "USD");
    private BillingPeriod _billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);
    private int _trialDays = 0;
    private int _trialPeriodDays = 0;
    private bool _isActive = true;
    private bool _isPublic = true;
    private int _sortOrder = 0;

    public static SubscriptionPlanTestDataBuilder Create()
    {
        return new SubscriptionPlanTestDataBuilder();
    }

    public SubscriptionPlanTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithPrice(Money price)
    {
        _price = price;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithBillingPeriod(BillingPeriod billingPeriod)
    {
        _billingPeriod = billingPeriod;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithTrialDays(int trialDays)
    {
        _trialDays = trialDays;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithTrialPeriodDays(int trialPeriodDays)
    {
        _trialPeriodDays = trialPeriodDays;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithIsPublic(bool isPublic)
    {
        _isPublic = isPublic;
        return this;
    }

    public SubscriptionPlanTestDataBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public SubscriptionPlan Build()
    {
        var plan = SubscriptionPlan.Create(
            _tenantId,
            _name,
            _price.Amount,
            _price.Currency,
            _billingPeriod.Type,
            _description,
            _trialDays,
            _trialPeriodDays,
            null, // featuresJson
            null, // limitationsJson
            _sortOrder);

        // Set properties using reflection (private setters)
        SetPrivateProperty(plan, "Id", _id);

        if (_isActive)
            plan.Activate();
        else
            plan.Deactivate();

        plan.UpdateVisibility(_isPublic);

        return plan;
    }

    private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(obj, value);
    }
}
