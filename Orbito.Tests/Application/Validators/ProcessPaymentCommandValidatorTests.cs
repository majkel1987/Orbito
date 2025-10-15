using FluentAssertions;
using FluentValidation.TestHelper;
using Orbito.Application.Features.Payments.Commands.ProcessPayment;
using Orbito.Application.Validators;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Validators;

public class ProcessPaymentCommandValidatorTests : BaseTestFixture
{
    private readonly ProcessPaymentCommandValidator _validator;

    public ProcessPaymentCommandValidatorTests()
    {
        _validator = new ProcessPaymentCommandValidator();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "USD",
            ExternalTransactionId: "ch_test_123",
            PaymentMethod: "card",
            ExternalPaymentId: "pm_test_123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 0.00m, // Invalid amount
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: -50.00m, // Invalid negative amount
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptySubscriptionId_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: Guid.Empty, // Invalid empty ID
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SubscriptionId)
            .WithErrorMessage("Subscription ID is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyClientId_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: Guid.Empty, // Invalid empty ID
            Amount: 100.00m,
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientId)
            .WithErrorMessage("Client ID is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithInvalidCurrency_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "INVALID" // Invalid currency
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Unsupported currency");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyCurrency_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "" // Empty currency
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNullCurrency_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: null! // Null currency
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidCurrencies_ShouldPass()
    {
        // Arrange
        var validCurrencies = new[] { "USD", "EUR", "PLN", "GBP", "CHF" };

        foreach (var currency in validCurrencies)
        {
            var command = new ProcessPaymentCommand(
                SubscriptionId: TestSubscriptionId,
                ClientId: TestClientId,
                Amount: 100.00m,
                Currency: currency
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Currency);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithLargeAmount_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 999999.99m, // Large but valid amount
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithSmallAmount_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 0.50m, // Small but valid amount
            Currency: "USD"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "USD",
            ExternalTransactionId: null, // Optional field
            PaymentMethod: null, // Optional field
            ExternalPaymentId: null // Optional field
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithAllFields_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            SubscriptionId: TestSubscriptionId,
            ClientId: TestClientId,
            Amount: 100.00m,
            Currency: "USD",
            ExternalTransactionId: "ch_test_123",
            PaymentMethod: "card",
            ExternalPaymentId: "pm_test_123"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
