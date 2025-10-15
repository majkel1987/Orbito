using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

/// <summary>
/// Builder for creating PaymentMethod test data with fluent API
/// </summary>
public class PaymentMethodTestDataBuilder
{
    private Guid _id = Guid.NewGuid();
    private TenantId _tenantId = TenantId.New();
    private Guid _clientId = Guid.NewGuid();
    private PaymentMethodType _type = PaymentMethodType.Card;
    private string _token = "tok_test_123456789";
    private string? _lastFourDigits = "4242";
    private DateTime? _expiryDate;
    private bool _isDefault = false;

    public static PaymentMethodTestDataBuilder Create() => new();

    public PaymentMethodTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PaymentMethodTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public PaymentMethodTestDataBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public PaymentMethodTestDataBuilder WithType(PaymentMethodType type)
    {
        _type = type;
        return this;
    }

    public PaymentMethodTestDataBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public PaymentMethodTestDataBuilder WithLastFourDigits(string lastFourDigits)
    {
        _lastFourDigits = lastFourDigits;
        return this;
    }

    public PaymentMethodTestDataBuilder WithExpiryDate(DateTime? expiryDate)
    {
        _expiryDate = expiryDate;
        return this;
    }

    public PaymentMethodTestDataBuilder AsDefault()
    {
        _isDefault = true;
        return this;
    }

    public PaymentMethodTestDataBuilder NotDefault()
    {
        _isDefault = false;
        return this;
    }

    public PaymentMethod Build()
    {
        var paymentMethod = PaymentMethod.Create(
            _tenantId,
            _clientId,
            _type,
            _token,
            _lastFourDigits,
            _expiryDate,
            _isDefault);
        
        // Set the specific ID if provided
        paymentMethod.Id = _id;
        
        return paymentMethod;
    }

    // Predefined scenarios for common test cases
    public static PaymentMethod ValidPaymentMethod()
        => Create().Build();

    public static PaymentMethod ExpiredPaymentMethod()
        => Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(-1)) // Expired last month
            .Build();

    public static PaymentMethod ExpiringSoonPaymentMethod()
        => Create()
            .WithExpiryDate(DateTime.UtcNow.AddDays(30)) // Expires in 30 days
            .Build();

    public static PaymentMethod ValidNotExpiredPaymentMethod()
        => Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(12)) // Valid for 12 months
            .Build();

    public static PaymentMethod DefaultPaymentMethod()
        => Create()
            .AsDefault()
            .Build();

    public static PaymentMethod CreditCardPaymentMethod()
        => Create()
            .WithType(PaymentMethodType.Card)
            .WithLastFourDigits("4242")
            .Build();

    public static PaymentMethod DebitCardPaymentMethod()
        => Create()
            .WithType(PaymentMethodType.Card)
            .WithLastFourDigits("5555")
            .Build();

    public static PaymentMethod BankAccountPaymentMethod()
        => Create()
            .WithType(PaymentMethodType.BankTransfer)
            .WithLastFourDigits("1234")
            .Build();

    public static PaymentMethod PaymentMethodWithEmptyToken()
        => Create()
            .WithToken("")
            .Build();

    public static PaymentMethod PaymentMethodWithNullToken()
        => Create()
            .WithToken(null!)
            .Build();

    public static PaymentMethod PaymentMethodWithoutExpiryDate()
        => Create()
            .WithExpiryDate(null)
            .Build();

    public static PaymentMethod PaymentMethodExpiringToday()
        => Create()
            .WithExpiryDate(DateTime.UtcNow.Date) // Expires today
            .Build();

    public static PaymentMethod PaymentMethodExpiringTomorrow()
        => Create()
            .WithExpiryDate(DateTime.UtcNow.Date.AddDays(1)) // Expires tomorrow
            .Build();
}
