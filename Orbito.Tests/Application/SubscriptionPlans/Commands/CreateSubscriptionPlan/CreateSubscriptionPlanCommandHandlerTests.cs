using FluentAssertions;
using Moq;
using Orbito.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.CreateSubscriptionPlan
{
    public class CreateSubscriptionPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateSubscriptionPlanCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CreateSubscriptionPlanCommandHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new CreateSubscriptionPlanCommandHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.Amount.Should().Be(command.Amount);
            result.Currency.Should().Be(command.Currency);
            result.BillingPeriod.Should().Be(createdPlan.BillingPeriod.ToString());
            result.TrialPeriodDays.Should().Be(command.TrialPeriodDays);
            result.IsActive.Should().BeTrue();
            result.IsPublic.Should().Be(command.IsPublic);
            result.SortOrder.Should().Be(command.SortOrder);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Amount.Should().Be(command.Amount);
            result.Currency.Should().Be(command.Currency);
            result.Description.Should().BeNull();
            result.TrialPeriodDays.Should().Be(0);
            result.IsActive.Should().BeTrue();
            result.IsPublic.Should().BeTrue();
            result.SortOrder.Should().Be(0);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.IsPublic.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.TrialPeriodDays.Should().Be(30);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Amount.Should().Be(command.Amount);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
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
            result.Should().NotBeNull();
            result.SortOrder.Should().Be(5);

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldThrowException()
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Tenant context is required to create subscription plan");

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var command = new CreateSubscriptionPlanCommand
            {
                Name = "Test Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Database error");

            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
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
                result.Should().NotBeNull();
                result.BillingPeriod.Should().Be(testCase.Expected);
            }
        }
    }
}
