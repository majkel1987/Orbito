using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.UpgradeSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.UpgradeSubscription
{
    public class UpgradeSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ILogger<UpgradeSubscriptionCommandHandler>> _loggerMock;
        private readonly UpgradeSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public UpgradeSubscriptionCommandHandlerTests()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _loggerMock = new Mock<ILogger<UpgradeSubscriptionCommandHandler>>();

            _handler = new UpgradeSubscriptionCommandHandler(
                _subscriptionServiceMock.Object,
                _subscriptionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidUpgradeableSubscription_ShouldUpgradeSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;
            updatedSubscription.CurrentPrice = Money.Create(49.99m, "USD");

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionServiceMock.Setup(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.NewPlanId.Should().Be(newPlanId);
            result.Status.Should().Be(updatedSubscription.Status.ToString());
            result.Message.Should().Be("Subscription upgraded successfully");

            _subscriptionServiceMock.Verify(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be("Subscription not found");

            _subscriptionServiceMock.Verify(x => x.CanUpgradeAsync(It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonUpgradeableSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be("Subscription cannot be upgraded to the specified plan");

            _subscriptionServiceMock.Verify(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithTrialSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = true; // Trial subscription cannot be upgraded

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be("Subscription cannot be upgraded to the specified plan");

            _subscriptionServiceMock.Verify(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be("Subscription cannot be upgraded to the specified plan");

            _subscriptionServiceMock.Verify(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateCorrectMoneyObject()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new UpgradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                NewPlanId = newPlanId,
                NewAmount = 99.99m,
                Currency = "EUR"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionServiceMock.Setup(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.Is<Money>(m => m.Amount == 99.99m && m.Currency == "EUR"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.Is<Money>(m => m.Amount == 99.99m && m.Currency == "EUR"), It.IsAny<CancellationToken>()), Times.Once);
        }

        private Subscription CreateTestSubscription()
        {
            return Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
        }
    }
}
