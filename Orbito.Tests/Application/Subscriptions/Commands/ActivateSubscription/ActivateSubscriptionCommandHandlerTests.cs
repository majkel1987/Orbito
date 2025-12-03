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
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<ActivateSubscriptionCommandHandler>> _loggerMock;
        private readonly ActivateSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public ActivateSubscriptionCommandHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<ActivateSubscriptionCommandHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new ActivateSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidSuspendedSubscription_ShouldActivateSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(subscription.Id);
            result.Value.Status.Should().Be(SubscriptionStatus.Active.ToString());

            subscription.Status.Should().Be(SubscriptionStatus.Active);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidPendingSubscription_ShouldActivateSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended; // Use Suspended instead of Pending

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Status.Should().Be(SubscriptionStatus.Active.ToString());

            subscription.Status.Should().Be(SubscriptionStatus.Active);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Subscription.NotFound");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithActiveSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Subscription.AlreadyActive");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Subscription.AlreadyActive");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Expired;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Subscription.AlreadyActive");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPastDueSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new ActivateSubscriptionCommand { SubscriptionId = subscriptionId, ClientId = _clientId };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.PastDue;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Subscription.AlreadyActive");

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
