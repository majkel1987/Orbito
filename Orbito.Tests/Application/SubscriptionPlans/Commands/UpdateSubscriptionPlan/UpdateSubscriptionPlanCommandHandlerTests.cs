using FluentAssertions;
using Moq;
using Orbito.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.UpdateSubscriptionPlan
{
    public class UpdateSubscriptionPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly UpdateSubscriptionPlanCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public UpdateSubscriptionPlanCommandHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new UpdateSubscriptionPlanCommandHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldUpdateSubscriptionPlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Old Name",
                19.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Old Description",
                7,
                7,
                null,
                null,
                1);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Updated Plan",
                Description = "Updated description",
                Amount = 39.99m,
                Currency = "EUR",
                BillingPeriodType = BillingPeriodType.Yearly,
                TrialDays = 14,
                TrialPeriodDays = 14,
                FeaturesJson = "{\"features\":[{\"name\":\"New Feature\",\"isEnabled\":true}]}",
                LimitationsJson = "{\"limitations\":[{\"name\":\"Users\",\"type\":1,\"numericValue\":10}]}",
                IsActive = true,
                IsPublic = false,
                SortOrder = 2
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be(command.Name);
            result.Description.Should().Be(command.Description);
            result.Amount.Should().Be(command.Amount);
            result.Currency.Should().Be(command.Currency);
            result.BillingPeriod.Should().Be("1 Yearly");
            result.TrialPeriodDays.Should().Be(command.TrialPeriodDays);
            result.IsActive.Should().Be(command.IsActive);
            result.IsPublic.Should().Be(command.IsPublic);
            result.SortOrder.Should().Be(command.SortOrder);
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithMinimalUpdates_ShouldUpdateOnlyProvidedFields()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Name",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Original Description",
                7,
                7,
                null,
                null,
                1);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Updated Name Only",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);
            result.Amount.Should().Be(command.Amount);
            result.Currency.Should().Be(command.Currency);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDeactivation_ShouldDeactivatePlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Active Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            existingPlan.Activate();

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Active Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                IsActive = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsActive.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithActivation_ShouldActivatePlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Inactive Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            existingPlan.Deactivate();

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Inactive Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                IsActive = true
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsActive.Should().BeTrue();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithVisibilityChange_ShouldUpdateVisibility()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Public Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            existingPlan.UpdateVisibility(true);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Public Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                IsPublic = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsPublic.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithFeaturesAndLimitationsUpdate_ShouldUpdateJsonProperties()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Plan with Features",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var newFeaturesJson = "{\"features\":[{\"name\":\"Updated Feature\",\"isEnabled\":true}]}";
            var newLimitationsJson = "{\"limitations\":[{\"name\":\"Updated Limit\",\"type\":1,\"numericValue\":20}]}";

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Plan with Features",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                FeaturesJson = newFeaturesJson,
                LimitationsJson = newLimitationsJson
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(command.Name);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithTrialPeriodUpdate_ShouldUpdateTrialPeriod()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Trial Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                trialDays: 7,
                trialPeriodDays: 7);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Trial Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                TrialDays = 30,
                TrialPeriodDays = 30
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TrialPeriodDays.Should().Be(30);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithSortOrderUpdate_ShouldUpdateSortOrder()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Sortable Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                sortOrder: 1);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Sortable Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly,
                SortOrder = 5
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.SortOrder.Should().Be(5);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldThrowException()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new UpdateSubscriptionPlanCommand
            {
                Id = Guid.NewGuid(),
                Name = "Test Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Tenant context is required to update subscription plan");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentPlan_ShouldThrowException()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Non-existent Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be($"Subscription plan with ID {planId} not found");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Test Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = BillingPeriodType.Monthly
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);
            _subscriptionPlanRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Database error");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(BillingPeriodType.Daily, "1 Daily")]
        [InlineData(BillingPeriodType.Weekly, "1 Weekly")]
        [InlineData(BillingPeriodType.Monthly, "1 Monthly")]
        [InlineData(BillingPeriodType.Yearly, "1 Yearly")]
        public async Task Handle_WithDifferentBillingPeriods_ShouldUpdateCorrectBillingPeriod(BillingPeriodType billingPeriodType, string expectedBillingPeriod)
        {
            // Arrange
            var planId = Guid.NewGuid();
            var existingPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var command = new UpdateSubscriptionPlanCommand
            {
                Id = planId,
                Name = "Test Plan",
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodType = billingPeriodType
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.BillingPeriod.Should().Be(expectedBillingPeriod);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
