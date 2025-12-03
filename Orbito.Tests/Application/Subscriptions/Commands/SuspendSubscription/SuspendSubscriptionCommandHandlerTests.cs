using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.SuspendSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.SuspendSubscription
{
    [Trait("Category", "Unit")]
    public class SuspendSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<SuspendSubscriptionCommandHandler>> _loggerMock;
        private readonly SuspendSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public SuspendSubscriptionCommandHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<SuspendSubscriptionCommandHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new SuspendSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidActiveSubscription_ShouldSuspendSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Reason = "Customer requested suspension"
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.Status.Should().Be(SubscriptionStatus.Suspended.ToString());
            result.Value.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            subscription.Status.Should().Be(SubscriptionStatus.Suspended);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");
            result.Error.Message.Should().Contain("not found");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithSuspendedSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Suspended;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotSuspend");
            result.Error.Message.Should().Contain("cannot be suspended");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Cancelled;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotSuspend");
            result.Error.Message.Should().Contain("cannot be suspended");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Expired;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotSuspend");
            result.Error.Message.Should().Contain("cannot be suspended");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPastDueSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.PastDue;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotSuspend");
            result.Error.Message.Should().Contain("cannot be suspended");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithPendingSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Pending;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotSuspend");
            result.Error.Message.Should().Contain("cannot be suspended");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNoTenantContext_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            // Create a new handler with no tenant context
            var tenantContextMock = new Mock<ITenantContext>();
            tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            var handler = new SuspendSubscriptionCommandHandler(
                _subscriptionRepositoryMock.Object,
                tenantContextMock.Object,
                _loggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
            result.Error.Message.Should().Contain("Tenant context");

            _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithDifferentTenantId_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var differentTenantId = TenantId.New();
            var subscription = Subscription.Create(
                differentTenantId,
                _clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");
            result.Error.Message.Should().Contain("not found");

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithReasonProvided_ShouldSuspendSuccessfully()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var reason = "Payment method expired - customer requested temporary suspension";
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Reason = reason
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.Status.Should().Be(SubscriptionStatus.Suspended.ToString());

            subscription.Status.Should().Be(SubscriptionStatus.Suspended);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapAllSubscriptionPropertiesCorrectly()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                planId,
                Money.Create(49.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                trialDays: 14);
            subscription.ExternalSubscriptionId = "sub_stripe_123";
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            var dto = result.Value;
            dto.Should().NotBeNull();
            dto.TenantId.Should().Be(_tenantId.Value);
            dto.ClientId.Should().Be(_clientId);
            dto.PlanId.Should().Be(planId);
            dto.Status.Should().Be(SubscriptionStatus.Suspended.ToString());
            dto.Amount.Should().Be(49.99m);
            dto.Currency.Should().Be("USD");
            dto.BillingPeriodValue.Should().Be(1);
            dto.BillingPeriodType.Should().Be(BillingPeriodType.Monthly.ToString());
            dto.IsInTrial.Should().BeTrue();
            dto.TrialEndDate.Should().NotBeNull();
            dto.ExternalSubscriptionId.Should().Be("sub_stripe_123");
            dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryOnce()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = CreateTestSubscription(subscriptionId);
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithActiveSubscriptionInTrial_ShouldSuspendSuccessfully()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new SuspendSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId
            };

            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                trialDays: 7);
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Status.Should().Be(SubscriptionStatus.Suspended.ToString());
            result.Value.IsInTrial.Should().BeTrue();

            subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        }

        private Subscription CreateTestSubscription(Guid? id = null)
        {
            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));

            if (id.HasValue)
            {
                subscription.Id = id.Value;
            }

            return subscription;
        }
    }
}
