using FluentAssertions;
using FluentValidation.TestHelper;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Application.Features.Payments.Commands.Validators;
using Orbito.Tests.Helpers;
using Xunit;

namespace Orbito.Tests.Application.Validators;

public class RefundPaymentCommandValidatorTests : BaseTestFixture
{
    private readonly RefundPaymentCommandValidator _validator;

    public RefundPaymentCommandValidatorTests()
    {
        _validator = new RefundPaymentCommandValidator();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyPaymentId_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.Empty, // Invalid empty ID
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PaymentId)
            .WithErrorMessage("Payment ID is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 0.00m, // Invalid amount
            Currency = "USD",
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Refund amount must be greater than zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = -50.00m, // Invalid negative amount
            Currency = "USD",
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Refund amount must be greater than zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyCurrency_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "", // Empty currency
            Reason = "Customer request"
        };

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
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = null!, // Null currency
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithInvalidCurrency_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "INVALID", // Invalid currency
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be a 3-letter code");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyReason_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "USD",
            Reason = "" // Empty reason
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Refund reason is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNullReason_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "USD",
            Reason = null! // Null reason
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Refund reason is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithTooLongReason_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "USD",
            Reason = new string('A', 501) // Too long reason (over 500 characters)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Refund reason cannot exceed 500 characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidCurrencies_ShouldPass()
    {
        // Arrange
        var validCurrencies = new[] { "USD", "EUR", "PLN", "GBP", "CHF" };

        foreach (var currency in validCurrencies)
        {
            var command = new RefundPaymentCommand
            {
                PaymentId = Guid.NewGuid(),
                Amount = 100.00m,
                Currency = currency,
                Reason = "Customer request"
            };

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
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 999999.99m, // Large but valid amount
            Currency = "USD",
            Reason = "Customer request"
        };

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
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 0.50m, // Small but valid amount
            Currency = "USD",
            Reason = "Customer request"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidReasonLength_ShouldPass()
    {
        // Arrange
        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100.00m,
            Currency = "USD",
            Reason = new string('A', 500) // Maximum allowed length
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithCommonRefundReasons_ShouldPass()
    {
        // Arrange
        var commonReasons = new[]
        {
            "Customer request",
            "Product defect",
            "Service not provided",
            "Duplicate charge",
            "Fraudulent transaction",
            "Merchant error"
        };

        foreach (var reason in commonReasons)
        {
            var command = new RefundPaymentCommand
            {
                PaymentId = Guid.NewGuid(),
                Amount = 100.00m,
                Currency = "USD",
                Reason = reason
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Reason);
        }
    }
}
