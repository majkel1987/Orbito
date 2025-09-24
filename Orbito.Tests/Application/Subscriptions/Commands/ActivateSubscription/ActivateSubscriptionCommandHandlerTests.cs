using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.ActivateSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.ActivateSubscription
{
    public class ActivateSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ILogger<ActivateSubscriptionCommandHandler>> _loggerMock;
        private readonly ActivateSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public ActivateSubscriptionCommandHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _loggerMock = new Mock<ILogger<ActivateSubscriptionCommandHandler>>();

            _handler = new ActivateSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidSuspendedSubscription_ShouldActivateSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Status.Should().Be(SubscriptionStatus.Active.ToString());
            result.Message.Should().Be("Subscription activated successfully");

            subscription.Status.Should().Be(SubscriptionStatus.Active);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidPendingSubscription_ShouldActivateSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended; // Use Suspended instead of Pending

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Status.Should().Be(SubscriptionStatus.Active.ToString());

            subscription.Status.Should().Be(SubscriptionStatus.Active);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be("Subscription not found");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithActiveSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be activated. Current status: {SubscriptionStatus.Active}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be activated. Current status: {SubscriptionStatus.Cancelled}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Expired;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be activated. Current status: {SubscriptionStatus.Expired}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPastDueSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.PastDue;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be activated. Current status: {SubscriptionStatus.PastDue}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
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
