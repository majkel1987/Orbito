using FluentAssertions;
using FluentValidation.TestHelper;
using Orbito.Application.Subscriptions.Commands.CreateSubscription;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandValidatorTests
    {
        private readonly CreateSubscriptionCommandValidator _validator;

        public CreateSubscriptionCommandValidatorTests()
        {
            _validator = new CreateSubscriptionCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly",
                TrialDays = 14
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyClientId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.Empty,
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ClientId)
                .WithErrorMessage("Client ID is required");
        }

        [Fact]
        public void Validate_WithEmptyPlanId_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.Empty,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PlanId)
                .WithErrorMessage("Plan ID is required");
        }

        [Fact]
        public void Validate_WithZeroAmount_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 0m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than 0");
        }

        [Fact]
        public void Validate_WithNegativeAmount_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = -10m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than 0");
        }

        [Fact]
        public void Validate_WithEmptyCurrency_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Currency is required");
        }

        [Fact]
        public void Validate_WithInvalidCurrencyLength_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "US",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Currency must be 3 characters long");
        }

        [Fact]
        public void Validate_WithZeroBillingPeriodValue_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 0,
                BillingPeriodType = "Monthly"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BillingPeriodValue)
                .WithErrorMessage("Billing period value must be greater than 0");
        }

        [Fact]
        public void Validate_WithEmptyBillingPeriodType_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = ""
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BillingPeriodType)
                .WithErrorMessage("Billing period type is required");
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("Hourly")]
        [InlineData("Quarterly")]
        [InlineData("BiYearly")]
        public void Validate_WithInvalidBillingPeriodType_ShouldHaveValidationError(string billingPeriodType)
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = billingPeriodType
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BillingPeriodType)
                .WithErrorMessage("Invalid billing period type. Must be Daily, Weekly, Monthly, or Yearly");
        }

        [Theory]
        [InlineData("Daily")]
        [InlineData("Weekly")]
        [InlineData("Monthly")]
        [InlineData("Yearly")]
        [InlineData("daily")]
        [InlineData("weekly")]
        [InlineData("monthly")]
        [InlineData("yearly")]
        public void Validate_WithValidBillingPeriodType_ShouldNotHaveValidationError(string billingPeriodType)
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = billingPeriodType
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.BillingPeriodType);
        }

        [Fact]
        public void Validate_WithNegativeTrialDays_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly",
                TrialDays = -1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TrialDays)
                .WithErrorMessage("Trial days must be 0 or greater");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(7)]
        [InlineData(14)]
        [InlineData(30)]
        [InlineData(90)]
        public void Validate_WithValidTrialDays_ShouldNotHaveValidationError(int trialDays)
        {
            // Arrange
            var command = new CreateSubscriptionCommand
            {
                ClientId = Guid.NewGuid(),
                PlanId = Guid.NewGuid(),
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly",
                TrialDays = trialDays
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TrialDays);
        }
    }
}
