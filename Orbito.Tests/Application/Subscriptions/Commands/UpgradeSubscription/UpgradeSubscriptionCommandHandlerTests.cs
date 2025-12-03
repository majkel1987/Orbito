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
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<UpgradeSubscriptionCommandHandler>> _loggerMock;
        private readonly UpgradeSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public UpgradeSubscriptionCommandHandlerTests()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<UpgradeSubscriptionCommandHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new UpgradeSubscriptionCommandHandler(
                _subscriptionServiceMock.Object,
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
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
                ClientId = _clientId,
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

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionServiceMock.Setup(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.IsAny<Money>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(updatedSubscription.Id);
            result.Value.PlanId.Should().Be(newPlanId);
            result.Value.Status.Should().Be(updatedSubscription.Status.ToString());
            result.Value.Amount.Should().Be(49.99m);
            result.Value.Currency.Should().Be("USD");

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
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");

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
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotUpgrade");

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
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = true; // Trial subscription cannot be upgraded

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotUpgrade");

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
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 49.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotUpgrade");

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
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 99.99m,
                Currency = "EUR"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;
            updatedSubscription.CurrentPrice = Money.Create(99.99m, "EUR");

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanUpgradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionServiceMock.Setup(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.Is<Money>(m => m.Amount == 99.99m && m.Currency == "EUR"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Amount.Should().Be(99.99m);
            result.Value.Currency.Should().Be("EUR");

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
