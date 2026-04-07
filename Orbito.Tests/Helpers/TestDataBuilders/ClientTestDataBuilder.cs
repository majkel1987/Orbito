using System.Reflection;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

public class ClientTestDataBuilder
{
    private Guid _id = Guid.NewGuid();
    private TenantId _tenantId = TenantId.New();
    private Guid _userId = Guid.NewGuid();
    private string _companyName = "Test Company";
    private string _email = "test@example.com";
    private string _phoneNumber = "+1234567890";
    private string _address = "123 Test Street";
    private string _city = "Test City";
    private string _postalCode = "12345";
    private string _country = "US";
    private bool _isActive = true;

    public static ClientTestDataBuilder Create()
    {
        return new ClientTestDataBuilder();
    }

    public ClientTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ClientTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public ClientTestDataBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ClientTestDataBuilder WithCompanyName(string companyName)
    {
        _companyName = companyName;
        return this;
    }

    public ClientTestDataBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ClientTestDataBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public ClientTestDataBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public ClientTestDataBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public ClientTestDataBuilder WithPostalCode(string postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    public ClientTestDataBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public ClientTestDataBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Client Build()
    {
        var client = Client.CreateWithUser(_tenantId, _userId, _companyName);

        // Set properties using reflection (private setters)
        SetPrivateProperty(client, "Id", _id);
        client.SetPhone(_phoneNumber);

        if (!_isActive)
        {
            client.Deactivate();
        }

        return client;
    }

    private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
    {
        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(obj, value);
    }
}
