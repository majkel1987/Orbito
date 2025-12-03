using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.DowngradeSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.DowngradeSubscription
{
    [Trait("Category", "Unit")]
    public class DowngradeSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<DowngradeSubscriptionCommandHandler>> _loggerMock;
        private readonly DowngradeSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public DowngradeSubscriptionCommandHandlerTests()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<DowngradeSubscriptionCommandHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new DowngradeSubscriptionCommandHandler(
                _subscriptionServiceMock.Object,
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidDowngradeableSubscription_ShouldDowngradeSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;
            updatedSubscription.CurrentPrice = Money.Create(19.99m, "USD");

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
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
            result.Value.Amount.Should().Be(19.99m);
            result.Value.Currency.Should().Be("USD");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");

            _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithSubscriptionFromDifferentTenant_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var differentTenantId = TenantId.New();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.TenantId = differentTenantId; // Different tenant

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonDowngradeableSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotDowngrade");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithTrialSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = true; // Trial subscription may not be downgradeable

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotDowngrade");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotDowngrade");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithSuspendedSubscription_ShouldReturnFailureResult()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotDowngrade");

            _subscriptionServiceMock.Verify(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                It.IsAny<Subscription>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateCorrectMoneyObject()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 9.99m,
                Currency = "EUR"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;
            updatedSubscription.CurrentPrice = Money.Create(9.99m, "EUR");

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionServiceMock.Setup(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.Is<Money>(m => m.Amount == 9.99m && m.Currency == "EUR"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Amount.Should().Be(9.99m);
            result.Value.Currency.Should().Be("EUR");

            _subscriptionServiceMock.Verify(x => x.ProcessSubscriptionChangeAsync(
                subscription, newPlanId, It.Is<Money>(m => m.Amount == 9.99m && m.Currency == "EUR"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldMapAllSubscriptionDtoProperties()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var newPlanId = Guid.NewGuid();
            var command = new DowngradeSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                NewPlanId = newPlanId,
                NewAmount = 19.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.PlanId = newPlanId;
            updatedSubscription.CurrentPrice = Money.Create(19.99m, "USD");
            updatedSubscription.Status = SubscriptionStatus.Active;
            updatedSubscription.StartDate = DateTime.UtcNow.AddDays(-30);
            updatedSubscription.EndDate = DateTime.UtcNow.AddDays(30);
            updatedSubscription.NextBillingDate = DateTime.UtcNow.AddDays(30);
            updatedSubscription.IsInTrial = false;
            updatedSubscription.TrialEndDate = null;
            updatedSubscription.ExternalSubscriptionId = "ext_sub_123";
            updatedSubscription.CreatedAt = DateTime.UtcNow.AddDays(-30);
            updatedSubscription.CancelledAt = null;
            updatedSubscription.UpdatedAt = DateTime.UtcNow;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);
            _subscriptionServiceMock.Setup(x => x.CanDowngradeAsync(subscription, newPlanId, It.IsAny<CancellationToken>()))
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
            result.Value.TenantId.Should().Be(updatedSubscription.TenantId.Value);
            result.Value.ClientId.Should().Be(updatedSubscription.ClientId);
            result.Value.PlanId.Should().Be(newPlanId);
            result.Value.Status.Should().Be(updatedSubscription.Status.ToString());
            result.Value.Amount.Should().Be(19.99m);
            result.Value.Currency.Should().Be("USD");
            result.Value.BillingPeriodValue.Should().Be(updatedSubscription.BillingPeriod.Value);
            result.Value.BillingPeriodType.Should().Be(updatedSubscription.BillingPeriod.Type.ToString());
            result.Value.StartDate.Should().Be(updatedSubscription.StartDate);
            result.Value.EndDate.Should().Be(updatedSubscription.EndDate);
            result.Value.NextBillingDate.Should().Be(updatedSubscription.NextBillingDate);
            result.Value.IsInTrial.Should().Be(updatedSubscription.IsInTrial);
            result.Value.TrialEndDate.Should().Be(updatedSubscription.TrialEndDate);
            result.Value.ExternalSubscriptionId.Should().Be(updatedSubscription.ExternalSubscriptionId);
            result.Value.CreatedAt.Should().Be(updatedSubscription.CreatedAt);
            result.Value.CancelledAt.Should().Be(updatedSubscription.CancelledAt);
            result.Value.UpdatedAt.Should().Be(updatedSubscription.UpdatedAt);
        }

        private Subscription CreateTestSubscription()
        {
            return Subscription.Create(
                _tenantId,
                _clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
        }
    }
}

