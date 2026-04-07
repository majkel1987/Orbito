using System.Reflection;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

public class SubscriptionTestDataBuilder
{
    private Guid _id = Guid.NewGuid();
    private TenantId _tenantId = TenantId.New();
    private Guid _clientId = Guid.NewGuid();
    private Guid _planId = Guid.NewGuid();
    private Money _price = Money.Create(29.99m, "USD");
    private BillingPeriod _billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);
    private SubscriptionStatus _status = SubscriptionStatus.Active;
    private DateTime? _startDate = DateTime.UtcNow;
    private DateTime? _endDate = null;
    private bool _isInTrial = false;
    private DateTime? _trialEndDate = null;
    private int _trialDays = 0;
    private SubscriptionPlan? _plan;

    public static SubscriptionTestDataBuilder Create()
    {
        return new SubscriptionTestDataBuilder();
    }

    public SubscriptionTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SubscriptionTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public SubscriptionTestDataBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public SubscriptionTestDataBuilder WithPlanId(Guid planId)
    {
        _planId = planId;
        return this;
    }

    public SubscriptionTestDataBuilder WithPrice(Money price)
    {
        _price = price;
        return this;
    }

    public SubscriptionTestDataBuilder WithBillingPeriod(BillingPeriod billingPeriod)
    {
        _billingPeriod = billingPeriod;
        return this;
    }

    public SubscriptionTestDataBuilder WithStatus(SubscriptionStatus status)
    {
        _status = status;
        return this;
    }

    public SubscriptionTestDataBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public SubscriptionTestDataBuilder WithEndDate(DateTime? endDate)
    {
        _endDate = endDate;
        return this;
    }

    public SubscriptionTestDataBuilder WithIsInTrial(bool isInTrial)
    {
        _isInTrial = isInTrial;
        return this;
    }

    public SubscriptionTestDataBuilder WithTrialEndDate(DateTime? trialEndDate)
    {
        _trialEndDate = trialEndDate;
        return this;
    }

    public SubscriptionTestDataBuilder WithTrialDays(int trialDays)
    {
        _trialDays = trialDays;
        return this;
    }

    public SubscriptionTestDataBuilder WithPlan(SubscriptionPlan plan)
    {
        _plan = plan;
        return this;
    }

    public Subscription Build()
    {
        var subscription = Subscription.Create(_tenantId, _clientId, _planId, _price, _billingPeriod, _trialDays);

        // Set properties using reflection (private setters)
        SetPrivateProperty(subscription, "Id", _id);
        SetPrivateProperty(subscription, "Status", _status);
        SetPrivateProperty(subscription, "StartDate", _startDate ?? DateTime.UtcNow);
        SetPrivateProperty(subscription, "EndDate", _endDate);
        SetPrivateProperty(subscription, "IsInTrial", _isInTrial);
        SetPrivateProperty(subscription, "TrialEndDate", _trialEndDate);

        // Set the plan if provided
        if (_plan != null)
        {
            SetPrivateProperty(subscription, "Plan", _plan);
        }

        return subscription;
    }

    private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(obj, value);
    }
}
