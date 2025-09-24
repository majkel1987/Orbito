using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Commands.CreateSubscription;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Commands.CreateSubscription
{
    public class CreateSubscriptionCommandHandlerTests
    {
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ILogger<CreateSubscriptionCommandHandler>> _loggerMock;
        private readonly CreateSubscriptionCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CreateSubscriptionCommandHandlerTests()
        {
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _loggerMock = new Mock<ILogger<CreateSubscriptionCommandHandler>>();

            _handler = new CreateSubscriptionCommandHandler(
                _subscriptionServiceMock.Object,
                _clientRepositoryMock.Object,
                _subscriptionPlanRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithValidCommand_ShouldCreateSubscription()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly",
                TrialDays = 14
            };

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            var subscription = Subscription.Create(
                _tenantId,
                clientId,
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                14);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionServiceMock.Setup(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 14, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.SubscriptionId.Should().Be(subscription.Id);
            result.ClientId.Should().Be(clientId);
            result.PlanId.Should().Be(planId);
            result.Status.Should().Be(SubscriptionStatus.Active.ToString());
            result.IsInTrial.Should().BeTrue();
            result.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
            result.Message.Should().Be("Subscription created successfully");

            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 14, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNonExistentClient_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be($"Client with ID {clientId} not found");
            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNonExistentPlan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be($"Plan with ID {planId} not found");
            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithInactivePlan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly"
            };

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Deactivate(); // Deactivate the plan

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be($"Plan with ID {planId} is not active");
            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithZeroTrialDays_ShouldCreateSubscriptionWithoutTrial()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 29.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Monthly",
                TrialDays = 0
            };

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            var subscription = Subscription.Create(
                _tenantId,
                clientId,
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                0);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionServiceMock.Setup(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsInTrial.Should().BeFalse();
            result.TrialEndDate.Should().BeNull();

            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 0, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithYearlyBillingPeriod_ShouldCreateSubscriptionWithYearlyBilling()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var command = new CreateSubscriptionCommand
            {
                ClientId = clientId,
                PlanId = planId,
                Amount = 299.99m,
                Currency = "USD",
                BillingPeriodValue = 1,
                BillingPeriodType = "Yearly",
                TrialDays = 0
            };

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 299.99m, "USD", BillingPeriodType.Yearly);
            plan.Activate();

            var subscription = Subscription.Create(
                _tenantId,
                clientId,
                planId,
                Money.Create(299.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Yearly),
                0);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionServiceMock.Setup(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.SubscriptionId.Should().Be(subscription.Id);

            _subscriptionServiceMock.Verify(x => x.CreateSubscriptionAsync(
                clientId, planId, It.IsAny<Money>(), It.IsAny<BillingPeriod>(), 0, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
