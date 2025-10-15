using FluentAssertions;
using FluentValidation.TestHelper;
using Orbito.Application.Features.PaymentMethods.Commands;
using Orbito.Domain.Enums;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Validators;

public class AddPaymentMethodCommandValidatorTests : BaseTestFixture
{
    private readonly AddPaymentMethodCommandValidator _validator;

    public AddPaymentMethodCommandValidatorTests()
    {
        _validator = new AddPaymentMethodCommandValidator();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            SetAsDefault = false
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyClientId_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = Guid.Empty, // Invalid empty ID
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientId)
            .WithErrorMessage("Client ID is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithInvalidPaymentMethodType_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = (PaymentMethodType)999, // Invalid enum value
            Token = "tok_test_123456789",
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid payment method type");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "", // Empty token
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Payment method token is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNullToken_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = null!, // Null token
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Payment method token is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithTooLongToken_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = new string('A', 501), // Too long token (over 500 characters)
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Token cannot exceed 500 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithInvalidLastFourDigits_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "123" // Invalid - not 4 digits
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits)
            .WithErrorMessage("Last four digits must be exactly 4 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNonNumericLastFourDigits_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "ABCD" // Invalid - not numeric
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastFourDigits)
            .WithErrorMessage("Last four digits must contain only numbers");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithInvalidExpiryMonth_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 13, // Invalid month
            ExpiryYear = 2025
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month must be between 1 and 12");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithZeroExpiryMonth_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 0, // Invalid month
            ExpiryYear = 2025
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month must be between 1 and 12");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithPastExpiryYear_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = 2020 // Past year
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage($"Expiry year must be {DateTime.UtcNow.Year} or later");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithFutureExpiryYear_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 21 // Too far in future
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage($"Expiry year cannot be more than 20 years in the future");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithExpiryMonthWithoutYear_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = null // Missing year
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
            .WithErrorMessage("Expiry year is required when expiry month is provided");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithExpiryYearWithoutMonth_ShouldFail()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123456789",
            LastFourDigits = "4242",
            ExpiryMonth = null, // Missing month
            ExpiryYear = 2025
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth)
            .WithErrorMessage("Expiry month is required when expiry year is provided");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidPaymentMethodTypes_ShouldPass()
    {
        // Arrange
        var validTypes = new[]
        {
            PaymentMethodType.Card,
            PaymentMethodType.Card,
            PaymentMethodType.BankTransfer
        };

        foreach (var type in validTypes)
        {
            var command = new AddPaymentMethodCommand
            {
                ClientId = TestClientId,
                Type = type,
                Token = "tok_test_123456789",
                LastFourDigits = "4242"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Type);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidLastFourDigits_ShouldPass()
    {
        // Arrange
        var validDigits = new[] { "1234", "5678", "9012", "3456" };

        foreach (var digits in validDigits)
        {
            var command = new AddPaymentMethodCommand
            {
                ClientId = TestClientId,
                Type = PaymentMethodType.Card,
                Token = "tok_test_123456789",
                LastFourDigits = digits
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.LastFourDigits);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidExpiryDates_ShouldPass()
    {
        // Arrange
        var validExpiryDates = new[]
        {
            (12, DateTime.UtcNow.Year), // Current year
            (1, DateTime.UtcNow.Year + 1), // Next year
            (6, DateTime.UtcNow.Year + 5), // 5 years from now
            (12, DateTime.UtcNow.Year + 20) // Maximum allowed
        };

        foreach (var (month, year) in validExpiryDates)
        {
            var command = new AddPaymentMethodCommand
            {
                ClientId = TestClientId,
                Type = PaymentMethodType.Card,
                Token = "tok_test_123456789",
                LastFourDigits = "4242",
                ExpiryMonth = month,
                ExpiryYear = year
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ExpiryMonth);
            result.ShouldNotHaveValidationErrorFor(x => x.ExpiryYear);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.BankTransfer,
            Token = "tok_test_123456789",
            LastFourDigits = null, // Optional field
            ExpiryMonth = null, // Optional field
            ExpiryYear = null, // Optional field
            SetAsDefault = false
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithMaximumTokenLength_ShouldPass()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = new string('A', 500), // Maximum allowed length
            LastFourDigits = "4242"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}
