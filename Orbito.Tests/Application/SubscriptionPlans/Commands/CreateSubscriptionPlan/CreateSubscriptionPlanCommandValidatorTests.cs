using FluentAssertions;
using FluentValidation.TestHelper;
using Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public class CreateSubscriptionPlanCommandValidatorTests
    {
        private readonly CreateSubscriptionPlanCommandValidator _validator;

        public CreateSubscriptionPlanCommandValidatorTests()
        {
            _validator = new CreateSubscriptionPlanCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Description = "Basic features for small businesses",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = 14,
                TrialPeriodDays = 14,
                SortOrder = 1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyName_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Plan name is required");
        }

        [Fact]
        public void Validate_WithNullName_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = null!,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Plan name is required");
        }

        [Fact]
        public void Validate_WithNameTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = new string('A', 201), // 201 characters
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Plan name cannot exceed 200 characters");
        }

        [Fact]
        public void Validate_WithNameAtMaxLength_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = new string('A', 200), // Exactly 200 characters
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Validate_WithDescriptionTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Description = new string('A', 1001), // 1001 characters
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Description cannot exceed 1000 characters");
        }

        [Fact]
        public void Validate_WithDescriptionAtMaxLength_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Description = new string('A', 1000), // Exactly 1000 characters
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_WithNullDescription_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Description = null,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_WithNegativeAmount_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = -1m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Amount must be greater than or equal to 0");
        }

        [Fact]
        public void Validate_WithZeroAmount_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Free Plan",
                Amount = 0m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Validate_WithPositiveAmount_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Validate_WithEmptyCurrency_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "",
                BillingPeriodType = BillingPeriodType.Monthly
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
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "US", // 2 characters instead of 3
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Currency must be 3 characters (ISO 4217)");
        }

        [Fact]
        public void Validate_WithValidCurrency_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Currency);
        }

        [Fact]
        public void Validate_WithInvalidBillingPeriodType_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = (BillingPeriodType)999 // Invalid enum value
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BillingPeriodType)
                .WithErrorMessage("Invalid billing period type");
        }

        [Theory]
        [InlineData(BillingPeriodType.Daily)]
        [InlineData(BillingPeriodType.Weekly)]
        [InlineData(BillingPeriodType.Monthly)]
        [InlineData(BillingPeriodType.Yearly)]
        public void Validate_WithValidBillingPeriodTypes_ShouldNotHaveValidationError(BillingPeriodType billingPeriodType)
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
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
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = -1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TrialDays)
                .WithErrorMessage("Trial days must be greater than or equal to 0");
        }

        [Fact]
        public void Validate_WithZeroTrialDays_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = 0
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TrialDays);
        }

        [Fact]
        public void Validate_WithPositiveTrialDays_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = 14
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TrialDays);
        }

        [Fact]
        public void Validate_WithNegativeTrialPeriodDays_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialPeriodDays = -1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TrialPeriodDays)
                .WithErrorMessage("Trial period days must be greater than or equal to 0");
        }

        [Fact]
        public void Validate_WithZeroTrialPeriodDays_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialPeriodDays = 0
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TrialPeriodDays);
        }

        [Fact]
        public void Validate_WithPositiveTrialPeriodDays_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialPeriodDays = 30
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.TrialPeriodDays);
        }

        [Fact]
        public void Validate_WithNegativeSortOrder_ShouldHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                SortOrder = -1
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SortOrder)
                .WithErrorMessage("Sort order must be greater than or equal to 0");
        }

        [Fact]
        public void Validate_WithZeroSortOrder_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                SortOrder = 0
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }

        [Fact]
        public void Validate_WithPositiveSortOrder_ShouldNotHaveValidationError()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Basic Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                SortOrder = 5
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
        }

        [Fact]
        public void Validate_WithMultipleValidationErrors_ShouldHaveAllErrors()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "", // Empty name
                Amount = -1m, // Negative amount
                Currency = "US", // Invalid currency length
                BillingPeriodType = (BillingPeriodType)999, // Invalid billing period
                TrialDays = -1, // Negative trial days
                TrialPeriodDays = -1, // Negative trial period days
                SortOrder = -1 // Negative sort order
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
            result.ShouldHaveValidationErrorFor(x => x.Currency);
            result.ShouldHaveValidationErrorFor(x => x.BillingPeriodType);
            result.ShouldHaveValidationErrorFor(x => x.TrialDays);
            result.ShouldHaveValidationErrorFor(x => x.TrialPeriodDays);
            result.ShouldHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}
