using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers.Assertions;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Domain.Entities;

public class PaymentMethodTests
{
    private readonly TenantId _tenantId = TenantId.New();
    private readonly Guid _clientId = Guid.NewGuid();

    #region Creation Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithValidData_ShouldCreatePaymentMethod()
    {
        // Arrange
        var type = PaymentMethodType.Card;
        var token = "tok_test_123456789";
        var lastFourDigits = "4242";
        var expiryDate = DateTime.UtcNow.AddMonths(12);

        // Act
        var paymentMethod = PaymentMethod.Create(_tenantId, _clientId, type, token, lastFourDigits, expiryDate);

        // Assert
        paymentMethod.Should().NotBeNull();
        paymentMethod.TenantId.Should().Be(_tenantId);
        paymentMethod.ClientId.Should().Be(_clientId);
        paymentMethod.Type.Should().Be(type);
        paymentMethod.Token.Should().Be(token);
        paymentMethod.LastFourDigits.Should().Be(lastFourDigits);
        paymentMethod.ExpiryDate.Should().Be(expiryDate);
        paymentMethod.IsDefault.Should().BeFalse();
        paymentMethod.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        paymentMethod.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithExpiryDate_ShouldSetExpiryDate()
    {
        // Arrange
        var expiryDate = DateTime.UtcNow.AddMonths(6);

        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(expiryDate)
            .Build();

        // Assert
        paymentMethod.ExpiryDate.Should().Be(expiryDate);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_AsDefault_ShouldSetIsDefault()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .AsDefault()
            .Build();

        // Assert
        paymentMethod.IsDefault.Should().BeTrue();
        paymentMethod.ShouldBeDefaultPaymentMethod();
    }

    #endregion

    #region Token Management Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateToken_WithValidToken_ShouldUpdateToken()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create().Build();
        var newToken = "tok_new_987654321";

        // Act
        paymentMethod.UpdateToken(newToken);

        // Assert
        paymentMethod.Token.Should().Be(newToken);
        paymentMethod.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateToken_WithNullToken_ShouldThrowArgumentException()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create().Build();

        // Act & Assert
        var action = () => paymentMethod.UpdateToken(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Token cannot be null or empty*")
            .WithParameterName("newToken");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateToken_ShouldUpdateTimestamp()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create().Build();
        var originalUpdatedAt = paymentMethod.UpdatedAt;
        var newToken = "tok_new_987654321";

        // Act
        paymentMethod.UpdateToken(newToken);

        // Assert
        paymentMethod.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region Default Management Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void SetAsDefault_ShouldSetIsDefaultTrue()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .NotDefault()
            .Build();

        // Act
        paymentMethod.SetAsDefault();

        // Assert
        paymentMethod.IsDefault.Should().BeTrue();
        paymentMethod.ShouldBeDefaultPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAsDefault_ShouldSetIsDefaultFalse()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .AsDefault()
            .Build();

        // Act
        paymentMethod.RemoveAsDefault();

        // Assert
        paymentMethod.IsDefault.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SetAsDefault_ShouldUpdateTimestamp()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create().Build();
        var originalUpdatedAt = paymentMethod.UpdatedAt;

        // Act
        paymentMethod.SetAsDefault();

        // Assert
        paymentMethod.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAsDefault_ShouldUpdateTimestamp()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .AsDefault()
            .Build();
        var originalUpdatedAt = paymentMethod.UpdatedAt;

        // Act
        paymentMethod.RemoveAsDefault();

        // Assert
        paymentMethod.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void IsExpired_BeforeExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(6))
            .Build();

        // Act
        var result = paymentMethod.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsExpired_AfterExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(-1))
            .Build();

        // Act
        var result = paymentMethod.IsExpired();

        // Assert
        result.Should().BeTrue();
        paymentMethod.ShouldBeExpiredPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsExpired_OnLastDayOfMonth_ShouldReturnFalse()
    {
        // Arrange
        var lastDayOfCurrentMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month));
        
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(lastDayOfCurrentMonth)
            .Build();

        // Act
        var result = paymentMethod.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeUsed_ValidNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(12))
            .WithToken("tok_valid_123")
            .Build();

        // Act
        var result = paymentMethod.CanBeUsed();

        // Assert
        result.Should().BeTrue();
        paymentMethod.ShouldBeValidPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeUsed_ExpiredOrEmptyToken_ShouldReturnFalse()
    {
        // Arrange
        var expiredPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(-1))
            .Build();

        var emptyTokenPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithToken("")
            .Build();

        // Act & Assert
        expiredPaymentMethod.CanBeUsed().Should().BeFalse();
        emptyTokenPaymentMethod.CanBeUsed().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeUsed_WithNullToken_ShouldReturnFalse()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithToken(null!)
            .Build();

        // Act
        var result = paymentMethod.CanBeUsed();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeUsed_WithoutExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithExpiryDate(null)
            .WithToken("tok_valid_123")
            .Build();

        // Act
        var result = paymentMethod.CanBeUsed();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Predefined Scenarios Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidPaymentMethod_ShouldBeValidAndNotExpired()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.ValidPaymentMethod();

        // Assert
        paymentMethod.ShouldBeValidPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExpiredPaymentMethod_ShouldBeExpired()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.ExpiredPaymentMethod();

        // Assert
        paymentMethod.ShouldBeExpiredPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExpiringSoonPaymentMethod_ShouldNotBeExpired()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.ExpiringSoonPaymentMethod();

        // Assert
        paymentMethod.IsExpired().Should().BeFalse();
        paymentMethod.CanBeUsed().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DefaultPaymentMethod_ShouldBeDefault()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.DefaultPaymentMethod();

        // Assert
        paymentMethod.ShouldBeDefaultPaymentMethod();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreditCardPaymentMethod_ShouldHaveCorrectType()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.CreditCardPaymentMethod();

        // Assert
        paymentMethod.Type.Should().Be(PaymentMethodType.Card);
        paymentMethod.LastFourDigits.Should().Be("4242");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DebitCardPaymentMethod_ShouldHaveCorrectType()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.DebitCardPaymentMethod();

        // Assert
        paymentMethod.Type.Should().Be(PaymentMethodType.Card);
        paymentMethod.LastFourDigits.Should().Be("5555");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BankAccountPaymentMethod_ShouldHaveCorrectType()
    {
        // Act
        var paymentMethod = PaymentMethodTestDataBuilder.BankAccountPaymentMethod();

        // Assert
        paymentMethod.Type.Should().Be(PaymentMethodType.BankTransfer);
        paymentMethod.LastFourDigits.Should().Be("1234");
    }

    #endregion
}
