using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public class CreateSubscriptionPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CreateSubscriptionPlanCommandHandler>> _mockLogger;
        private readonly CreateSubscriptionPlanCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CreateSubscriptionPlanCommandHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CreateSubscriptionPlanCommandHandler>>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new CreateSubscriptionPlanCommandHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object,
                _mockLogger.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithValidCommand_ShouldCreateSubscriptionPlan()
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
                FeaturesJson = "{\"features\":[{\"name\":\"Basic Support\",\"isEnabled\":true}]}",
                LimitationsJson = "{\"limitations\":[{\"name\":\"Users\",\"type\":1,\"numericValue\":5}]}",
                IsPublic = true,
                SortOrder = 1
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType,
                command.Description,
                command.TrialDays,
                command.TrialPeriodDays,
                command.FeaturesJson,
                command.LimitationsJson,
                command.SortOrder);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().NotBeEmpty();
            result.Value.Name.Should().Be(command.Name);
            result.Value.Description.Should().Be(command.Description);
            result.Value.Amount.Should().Be(command.Amount);
            result.Value.Currency.Should().Be(command.Currency);
            result.Value.BillingPeriod.Should().Be(createdPlan.BillingPeriod.ToString());
            result.Value.TrialPeriodDays.Should().Be(command.TrialPeriodDays);
            result.Value.IsActive.Should().BeTrue();
            result.Value.IsPublic.Should().Be(command.IsPublic);
            result.Value.SortOrder.Should().Be(command.SortOrder);
            result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithMinimalCommand_ShouldCreateSubscriptionPlan()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Minimal Plan",
                Amount = 0m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(command.Name);
            result.Value.Amount.Should().Be(command.Amount);
            result.Value.Currency.Should().Be(command.Currency);
            result.Value.Description.Should().BeNull();
            result.Value.TrialPeriodDays.Should().Be(0);
            result.Value.IsActive.Should().BeTrue();
            result.Value.IsPublic.Should().BeTrue();
            result.Value.SortOrder.Should().Be(0);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithPrivatePlan_ShouldSetIsPublicToFalse()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Private Plan",
                Amount = 99.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Yearly,
                IsPublic = false
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType);
            createdPlan.UpdateVisibility(false);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IsPublic.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithTrialPeriod_ShouldSetTrialPeriodDays()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Trial Plan",
                Amount = 49.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = 30,
                TrialPeriodDays = 30
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType,
                trialDays: command.TrialDays,
                trialPeriodDays: command.TrialPeriodDays);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TrialPeriodDays.Should().Be(30);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithFeaturesAndLimitations_ShouldSetJsonProperties()
        {
            // Arrange
            var featuresJson = "{\"features\":[{\"name\":\"Advanced Analytics\",\"description\":\"Detailed reports\",\"isEnabled\":true}]}";
            var limitationsJson = "{\"limitations\":[{\"name\":\"Storage\",\"type\":1,\"numericValue\":1000,\"description\":\"GB of storage\"}]}";

            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Advanced Plan",
                Amount = 199.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Yearly,
                FeaturesJson = featuresJson,
                LimitationsJson = limitationsJson
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType,
                featuresJson: featuresJson,
                limitationsJson: limitationsJson);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(command.Name);
            result.Value.Amount.Should().Be(command.Amount);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithCustomSortOrder_ShouldSetSortOrder()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Premium Plan",
                Amount = 299.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Yearly,
                SortOrder = 5
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType,
                sortOrder: command.SortOrder);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.SortOrder.Should().Be(5);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Test Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithDifferentBillingPeriods_ShouldCreateCorrectBillingPeriod()
        {
            // Arrange
            var testCases = new[]
            {
                new { Type = BillingPeriodType.Daily, Expected = "1 Daily" },
                new { Type = BillingPeriodType.Weekly, Expected = "1 Weekly" },
                new { Type = BillingPeriodType.Monthly, Expected = "1 Monthly" },
                new { Type = BillingPeriodType.Yearly, Expected = "1 Yearly" }
            };

            foreach (var testCase in testCases)
            {
                var command = new CreateSubscriptionPlanCommand
                {
                    Name = $"Test Plan {testCase.Type}",
                    Amount = 29.99m,
                    Currency = "USD",
                    BillingPeriodType = testCase.Type
                };

                var createdPlan = SubscriptionPlan.Create(
                    _tenantId,
                    command.Name,
                    command.Amount,
                    command.Currency,
                    command.BillingPeriodType);

                _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(createdPlan);

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().NotBeNull();
                result.Value.BillingPeriod.Should().Be(testCase.Expected);
            }
        }


        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptyName_ShouldCreateSubscriptionPlan()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be("");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Negative Plan",
                Amount = -10.00m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain("Amount cannot be negative");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithVeryLongName_ShouldCreateSubscriptionPlan()
        {
            // Arrange
            var longName = new string('A', 1000);
            var command = new CreateSubscriptionPlanCommand
            {
                Name = longName,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            var createdPlan = SubscriptionPlan.Create(
                _tenantId,
                command.Name,
                command.Amount,
                command.Currency,
                command.BillingPeriodType);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(longName);
        }
    }
}
