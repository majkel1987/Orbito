using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.RenewSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.RenewSubscription
{
    [Trait("Category", "Unit")]
    public class RenewSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<RenewSubscriptionCommandHandler>> _loggerMock;
        private readonly RenewSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public RenewSubscriptionCommandHandlerTests()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<RenewSubscriptionCommandHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new RenewSubscriptionCommandHandler(
                _subscriptionServiceMock.Object,
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidSubscription_ShouldRenewSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = 29.99m;
            var currency = "USD";
            var externalPaymentId = "pi_test_123";
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = amount,
                Currency = currency,
                ExternalPaymentId = externalPaymentId
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Active;
            var nextBillingDate = DateTime.UtcNow.AddMonths(1);

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.Id = subscriptionId;
            updatedSubscription.Status = SubscriptionStatus.Active;
            updatedSubscription.NextBillingDate = nextBillingDate;

            _subscriptionRepositoryMock
                .Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.Is<Money>(m => m.Amount == amount && m.Currency == currency),
                    externalPaymentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _subscriptionRepositoryMock
                .SetupSequence(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription)
                .ReturnsAsync(updatedSubscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.Amount.Should().Be(updatedSubscription.CurrentPrice.Amount);
            result.Value.Currency.Should().Be(updatedSubscription.CurrentPrice.Currency);
            result.Value.NextBillingDate.Should().Be(nextBillingDate);

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.Is<Money>(m => m.Amount == amount && m.Currency == currency),
                    externalPaymentId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _subscriptionRepositoryMock.Verify(
                x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithValidSubscriptionWithoutExternalPaymentId_ShouldRenewSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = 29.99m;
            var currency = "USD";
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = amount,
                Currency = currency,
                ExternalPaymentId = null
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Active;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.Id = subscriptionId;
            updatedSubscription.Status = SubscriptionStatus.Active;
            updatedSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            _subscriptionRepositoryMock
                .SetupSequence(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription)
                .ReturnsAsync(updatedSubscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.Is<Money>(m => m.Amount == amount && m.Currency == currency),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.Is<Money>(m => m.Amount == amount && m.Currency == currency),
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
            result.Error.Message.Should().Contain("Tenant context");

            _subscriptionRepositoryMock.Verify(
                x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Money>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD"
            };

            _subscriptionRepositoryMock
                .Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");
            result.Error.Message.Should().Contain("not found");

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Money>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WithCrossTenantSubscription_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD"
            };

            var differentTenantId = TenantId.New();
            var subscription = CreateTestSubscription();
            subscription.TenantId = differentTenantId;

            _subscriptionRepositoryMock
                .Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.NotFound");
            result.Error.Message.Should().Contain("not found");

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Money>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenPaymentProcessingFails_ShouldReturnFailure()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD",
                ExternalPaymentId = "pi_test_123"
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Active;

            _subscriptionRepositoryMock
                .Setup(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.IsAny<Money>(),
                    command.ExternalPaymentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Subscription.CannotRenew");
            result.Error.Message.Should().Contain("cannot be renewed");

            _subscriptionServiceMock.Verify(
                x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.IsAny<Money>(),
                    command.ExternalPaymentId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _subscriptionRepositoryMock.Verify(
                x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithSuspendedSubscription_ShouldRenewSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Suspended;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.Id = subscriptionId;
            updatedSubscription.Status = SubscriptionStatus.Suspended;
            updatedSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            _subscriptionRepositoryMock
                .SetupSequence(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription)
                .ReturnsAsync(updatedSubscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.IsAny<Money>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
        }

        [Fact]
        public async Task Handle_WithPastDueSubscription_ShouldRenewSubscription()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = 29.99m,
                Currency = "USD"
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.PastDue;

            var updatedSubscription = CreateTestSubscription();
            updatedSubscription.Id = subscriptionId;
            updatedSubscription.Status = SubscriptionStatus.PastDue;
            updatedSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            _subscriptionRepositoryMock
                .SetupSequence(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription)
                .ReturnsAsync(updatedSubscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.IsAny<Money>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
        }

        [Fact]
        public async Task Handle_ShouldMapSubscriptionToDtoCorrectly()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var amount = 49.99m;
            var currency = "EUR";
            var command = new RenewSubscriptionCommand
            {
                SubscriptionId = subscriptionId,
                ClientId = _clientId,
                Amount = amount,
                Currency = currency
            };

            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                planId,
                Money.Create(amount, currency),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                14);

            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Active;
            var nextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscription.NextBillingDate = nextBillingDate;

            _subscriptionRepositoryMock
                .SetupSequence(x => x.GetByIdForClientAsync(subscriptionId, _clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription)
                .ReturnsAsync(subscription);

            _subscriptionServiceMock
                .Setup(x => x.ProcessPaymentAsync(
                    subscriptionId,
                    It.IsAny<Money>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.TenantId.Should().Be(_tenantId.Value);
            result.Value.ClientId.Should().Be(_clientId);
            result.Value.PlanId.Should().Be(planId);
            result.Value.Status.Should().Be(SubscriptionStatus.Active.ToString());
            result.Value.Amount.Should().Be(amount);
            result.Value.Currency.Should().Be(currency);
            result.Value.BillingPeriodValue.Should().Be(1);
            result.Value.BillingPeriodType.Should().Be(BillingPeriodType.Monthly.ToString());
            result.Value.NextBillingDate.Should().Be(nextBillingDate);
            result.Value.IsInTrial.Should().Be(subscription.IsInTrial);
            result.Value.TrialEndDate.Should().Be(subscription.TrialEndDate);
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

