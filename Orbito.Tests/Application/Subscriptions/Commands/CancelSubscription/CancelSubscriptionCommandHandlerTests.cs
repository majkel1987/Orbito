using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.CancelSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.CancelSubscription
{
    public class CancelSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ILogger<CancelSubscriptionCommandHandler>> _loggerMock;
        private readonly CancelSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CancelSubscriptionCommandHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _loggerMock = new Mock<ILogger<CancelSubscriptionCommandHandler>>();

            _handler = new CancelSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidActiveSubscription_ShouldCancelSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Status.Should().Be(SubscriptionStatus.Cancelled.ToString());
            result.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Message.Should().Be("Subscription cancelled successfully");

            subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
            subscription.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidSuspendedSubscription_ShouldCancelSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Status.Should().Be(SubscriptionStatus.Cancelled.ToString());
            result.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
            subscription.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be cancelled. Current status: {SubscriptionStatus.Cancelled}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Expired;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be cancelled. Current status: {SubscriptionStatus.Expired}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPastDueSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.PastDue;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be cancelled. Current status: {SubscriptionStatus.PastDue}");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPendingSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new CancelSubscriptionCommand { SubscriptionId = subscriptionId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Pending;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.SubscriptionId.Should().Be(subscriptionId);
            result.Message.Should().Be($"Subscription cannot be cancelled. Current status: {SubscriptionStatus.Pending}");

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
