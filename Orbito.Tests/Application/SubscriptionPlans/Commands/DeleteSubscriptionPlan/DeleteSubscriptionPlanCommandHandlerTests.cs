using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan
{
    public class DeleteSubscriptionPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<DeleteSubscriptionPlanCommandHandler>> _mockLogger;
        private readonly DeleteSubscriptionPlanCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public DeleteSubscriptionPlanCommandHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<DeleteSubscriptionPlanCommandHandler>>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new DeleteSubscriptionPlanCommandHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithSoftDeleteAndDeletablePlan_ShouldDeactivatePlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Deletable Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            subscriptionPlan.Activate();

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(planId);
            result.Value.IsDeleted.Should().BeTrue();
            result.Value.IsHardDelete.Should().BeFalse();
            result.Value.Message.Should().Be("Subscription plan deactivated");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(subscriptionPlan, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithHardDelete_ShouldPermanentlyDeletePlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Plan to Delete",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = true
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(planId);
            result.Value.IsDeleted.Should().BeTrue();
            result.Value.IsHardDelete.Should().BeTrue();
            result.Value.Message.Should().Be("Subscription plan permanently deleted");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(subscriptionPlan, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithSoftDeleteAndNonDeletablePlan_ShouldReturnFailure()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Non-Deletable Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add active subscription to make plan non-deletable
            var activeSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            activeSubscription.Status = SubscriptionStatus.Active;
            activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscriptionPlan.Subscriptions.Add(activeSubscription);

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(planId);
            result.Value.IsDeleted.Should().BeFalse();
            result.Value.IsHardDelete.Should().BeFalse();
            result.Value.Message.Should().Be("Cannot delete subscription plan with active subscriptions. Use hard delete to force deletion.");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithHardDeleteAndNonDeletablePlan_ShouldForceDelete()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Non-Deletable Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add active subscription to make plan non-deletable
            var activeSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            activeSubscription.Status = SubscriptionStatus.Active;
            activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscriptionPlan.Subscriptions.Add(activeSubscription);

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = true
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(planId);
            result.Value.IsDeleted.Should().BeTrue();
            result.Value.IsHardDelete.Should().BeTrue();
            result.Value.Message.Should().Be("Subscription plan permanently deleted");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(subscriptionPlan, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithSoftDeleteAndPlanWithInactiveSubscriptions_ShouldDeactivatePlan()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Plan with Inactive Subscriptions",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add cancelled subscription (should not prevent deletion)
            var cancelledSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            cancelledSubscription.Status = SubscriptionStatus.Cancelled;
            cancelledSubscription.StartDate = DateTime.UtcNow.AddMonths(-1);
            cancelledSubscription.EndDate = DateTime.UtcNow;
            subscriptionPlan.Subscriptions.Add(cancelledSubscription);

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(planId);
            result.Value.IsDeleted.Should().BeTrue();
            result.Value.IsHardDelete.Should().BeFalse();
            result.Value.Message.Should().Be("Subscription plan deactivated");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(subscriptionPlan, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new DeleteSubscriptionPlanCommand
            {
                Id = Guid.NewGuid(),
                HardDelete = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentPlan_ShouldReturnFailure()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId,
                HardDelete = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.SubscriptionPlan.NotFound);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithDefaultHardDeleteValue_ShouldPerformSoftDelete()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            subscriptionPlan.Activate();

            var command = new DeleteSubscriptionPlanCommand
            {
                Id = planId
                // HardDelete defaults to false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IsDeleted.Should().BeTrue();
            result.Value.IsHardDelete.Should().BeFalse();
            result.Value.Message.Should().Be("Subscription plan deactivated");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.UpdateAsync(subscriptionPlan, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
