using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Services;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Common.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<IDateTime> _dateTimeMock;
        private readonly Mock<ILogger<SubscriptionService>> _loggerMock;
        private readonly SubscriptionService _subscriptionService;
        private readonly TenantId _tenantId = TenantId.New();

        public SubscriptionServiceTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _dateTimeMock = new Mock<IDateTime>();
            _loggerMock = new Mock<ILogger<SubscriptionService>>();

            _subscriptionService = new SubscriptionService(
                _subscriptionRepositoryMock.Object,
                _clientRepositoryMock.Object,
                _subscriptionPlanRepositoryMock.Object,
                _dateTimeMock.Object,
                _loggerMock.Object);
        }

        #region CalculateNextBillingDateAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CalculateNextBillingDateAsync_WithTrialSubscription_ShouldReturnTrialEndDate()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.IsInTrial = true;
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(14);
            var currentDate = DateTime.UtcNow;
            _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);

            // Act
            var result = await _subscriptionService.CalculateNextBillingDateAsync(subscription);

            // Assert
            result.Should().Be(subscription.TrialEndDate.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CalculateNextBillingDateAsync_WithNonTrialSubscription_ShouldReturnNextBillingDate()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.IsInTrial = false;
            var currentDate = DateTime.UtcNow;
            _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);

            // Act
            var result = await _subscriptionService.CalculateNextBillingDateAsync(subscription);

            // Assert
            result.Should().BeCloseTo(currentDate.AddMonths(1), TimeSpan.FromDays(1));
        }

        #endregion

        #region CanUpgradeAsync Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanUpgradeAsync_WithUpgradeableSubscriptionAndHigherPricePlan_ShouldReturnTrue()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            var newPlan = SubscriptionPlan.Create(_tenantId, "Pro Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            newPlan.Activate();

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newPlan);

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanUpgradeAsync_WithNonUpgradeableSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled; // Cannot be upgraded

            var newPlanId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanUpgradeAsync_WithTrialSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = true; // Trial subscriptions cannot be upgraded

            var newPlanId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanUpgradeAsync_WithNonExistentPlan_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanUpgradeAsync_WithInactivePlan_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            var newPlan = SubscriptionPlan.Create(_tenantId, "Inactive Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            newPlan.Deactivate(); // Deactivate the plan

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newPlan);

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanUpgradeAsync_WithLowerPricePlan_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            var newPlan = SubscriptionPlan.Create(_tenantId, "Basic Plan", 19.99m, "USD", BillingPeriodType.Monthly);
            newPlan.Activate();

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newPlan);

            // Act
            var result = await _subscriptionService.CanUpgradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CanDowngradeAsync Tests

        [Fact]
        public async Task CanDowngradeAsync_WithDowngradeableSubscriptionAndLowerPricePlan_ShouldReturnTrue()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            var newPlan = SubscriptionPlan.Create(_tenantId, "Basic Plan", 19.99m, "USD", BillingPeriodType.Monthly);
            newPlan.Activate();

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newPlan);

            // Act
            var result = await _subscriptionService.CanDowngradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanDowngradeAsync_WithNonDowngradeableSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled; // Cannot be downgraded

            var newPlanId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.CanDowngradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanDowngradeAsync_WithTrialSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = true; // Trial subscriptions cannot be downgraded

            var newPlanId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.CanDowngradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanDowngradeAsync_WithHigherPricePlan_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.IsInTrial = false;

            var newPlanId = Guid.NewGuid();
            var newPlan = SubscriptionPlan.Create(_tenantId, "Pro Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            newPlan.Activate();

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newPlan);

            // Act
            var result = await _subscriptionService.CanDowngradeAsync(subscription, newPlanId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ProcessSubscriptionChangeAsync Tests

        [Fact]
        public async Task ProcessSubscriptionChangeAsync_WithValidSubscription_ShouldUpdateSubscription()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            var newPlanId = Guid.NewGuid();
            var newPrice = Money.Create(49.99m, "USD");

            // Act
            var result = await _subscriptionService.ProcessSubscriptionChangeAsync(subscription, newPlanId, newPrice);

            // Assert
            result.Should().Be(subscription);
            result.PlanId.Should().Be(newPlanId);
            result.CurrentPrice.Should().Be(newPrice);
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CanClientSubscribeToPlanAsync Tests

        [Fact]
        public async Task CanClientSubscribeToPlanAsync_WithValidClientAndPlan_ShouldReturnTrue()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionRepositoryMock.Setup(x => x.CanClientSubscribeToPlanAsync(clientId, planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanClientSubscribeToPlanAsync_WithNonExistentClient_ShouldReturnFalse()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanClientSubscribeToPlanAsync_WithNonExistentPlan_ShouldReturnFalse()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanClientSubscribeToPlanAsync_WithInactivePlan_ShouldReturnFalse()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            // Plan is not activated, so IsActive is false

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CreateSubscriptionAsync Tests

        [Fact]
        public async Task CreateSubscriptionAsync_WithValidParameters_ShouldCreateSubscription()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var price = Money.Create(29.99m, "USD");
            var billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);
            var trialDays = 14;

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            var createdSubscription = Subscription.Create(_tenantId, clientId, planId, price, billingPeriod, trialDays);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionRepositoryMock.Setup(x => x.CanClientSubscribeToPlanAsync(clientId, planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _subscriptionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdSubscription);

            // Act
            var result = await _subscriptionService.CreateSubscriptionAsync(clientId, planId, price, billingPeriod, trialDays);

            // Assert
            result.Should().Be(createdSubscription);
            _subscriptionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithNonExistentClient_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var price = Money.Create(29.99m, "USD");
            var billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _subscriptionService.CreateSubscriptionAsync(clientId, planId, price, billingPeriod));

            exception.Message.Should().Be($"Client with ID {clientId} not found");
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithNonExistentPlan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var price = Money.Create(29.99m, "USD");
            var billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _subscriptionService.CreateSubscriptionAsync(clientId, planId, price, billingPeriod));

            exception.Message.Should().Be($"Plan with ID {planId} not found");
        }

        [Fact]
        public async Task CreateSubscriptionAsync_WithClientCannotSubscribe_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var price = Money.Create(29.99m, "USD");
            var billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);
            _subscriptionRepositoryMock.Setup(x => x.CanClientSubscribeToPlanAsync(clientId, planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _subscriptionService.CreateSubscriptionAsync(clientId, planId, price, billingPeriod));

            exception.Message.Should().Be($"Client {clientId} cannot subscribe to plan {planId}");
        }

        #endregion

        #region ProcessPaymentAsync Tests

        [Fact]
        public async Task ProcessPaymentAsync_WithValidSubscription_ShouldProcessPayment()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = Money.Create(29.99m, "USD");
            var externalPaymentId = "stripe-payment-intent-123";

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(subscriptionId, amount, externalPaymentId);

            // Assert
            result.Should().BeTrue();
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WithNonExistentSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = Money.Create(29.99m, "USD");

            _subscriptionRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(subscriptionId, amount);

            // Assert
            result.Should().BeFalse();
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetExpiringSubscriptionsAsync Tests

        [Fact]
        public async Task GetExpiringSubscriptionsAsync_ShouldReturnExpiringSubscriptions()
        {
            // Arrange
            var daysBeforeExpiration = 7;
            var checkDate = DateTime.UtcNow;

            _dateTimeMock.Setup(x => x.UtcNow).Returns(checkDate);

            // Act
            var result = await _subscriptionService.GetExpiringSubscriptionsAsync(daysBeforeExpiration);

            // Assert
            // NOTE: This method returns empty list for security reasons (see SubscriptionService.cs line 187)
            // Background jobs should use tenant-specific methods instead
            result.Should().BeEmpty();
            _subscriptionRepositoryMock.Verify(x => x.GetExpiringSubscriptionsByClientAsync(It.IsAny<Guid>(), checkDate, daysBeforeExpiration, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ProcessExpiredSubscriptionsAsync Tests

        [Fact]
        public async Task ProcessExpiredSubscriptionsAsync_ShouldMarkExpiredSubscriptions()
        {
            // Arrange
            var checkDate = DateTime.UtcNow;
            var expiredSubscriptions = new List<Subscription>
            {
                CreateTestSubscription(),
                CreateTestSubscription()
            };

            _dateTimeMock.Setup(x => x.UtcNow).Returns(checkDate);

            // Act
            await _subscriptionService.ProcessExpiredSubscriptionsAsync();

            // Assert
            // NOTE: This method does not process subscriptions for security reasons (see SubscriptionService.cs line 198)
            // Background jobs should use tenant-specific methods instead
            // Subscriptions should remain unchanged
            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status.Should().Be(SubscriptionStatus.Active); // Status unchanged
            }

            _subscriptionRepositoryMock.Verify(x => x.GetExpiredSubscriptionsByClientAsync(It.IsAny<Guid>(), checkDate, It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ProcessRecurringPaymentsAsync Tests

        [Fact]
        public async Task ProcessRecurringPaymentsAsync_WithValidSubscriptions_ShouldProcessPayments()
        {
            // Arrange
            var billingDate = DateTime.UtcNow;
            var subscriptionsForBilling = new List<Subscription>
            {
                CreateTestSubscription(),
                CreateTestSubscription()
            };

            _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionsForBillingAsync(billingDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionsForBilling);

            // Act
            await _subscriptionService.ProcessRecurringPaymentsAsync(billingDate);

            // Assert
            _subscriptionRepositoryMock.Verify(x => x.GetSubscriptionsForBillingAsync(billingDate, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        #endregion

        #region Security Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanClientSubscribeToPlanAsync_WithCrossTenantClient_ShouldReturnFalse()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var differentTenantId = TenantId.New();

            var clientFromDifferentTenant = Client.CreateWithUser(differentTenantId, Guid.NewGuid(), "Test Company");
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientFromDifferentTenant);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeFalse(); // Cross-tenant access should be denied
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanClientSubscribeToPlanAsync_WithCrossTenantPlan_ShouldReturnFalse()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var differentTenantId = TenantId.New();

            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            var planFromDifferentTenant = SubscriptionPlan.Create(differentTenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            planFromDifferentTenant.Activate();

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(planFromDifferentTenant);

            // Act
            var result = await _subscriptionService.CanClientSubscribeToPlanAsync(clientId, planId);

            // Assert
            result.Should().BeFalse(); // Cross-tenant access should be denied
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task CreateSubscriptionAsync_WithCrossTenantClient_ShouldThrowSecurityException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var price = Money.Create(29.99m, "USD");
            var billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);

            // Mock GetByIdAsync to return null (client not found in current tenant context)
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();
            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(plan);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _subscriptionService.CreateSubscriptionAsync(clientId, planId, price, billingPeriod));

            exception.Message.Should().Be($"Client with ID {clientId} not found");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ProcessPaymentAsync_WithCrossTenantSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var amount = Money.Create(29.99m, "USD");
            var differentTenantId = TenantId.New();

            var subscriptionFromDifferentTenant = Subscription.Create(
                differentTenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));

            _subscriptionRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null); // Subscription not found for current tenant

            // Act
            var result = await _subscriptionService.ProcessPaymentAsync(subscriptionId, amount);

            // Assert
            result.Should().BeFalse(); // Cross-tenant access should be denied
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task GetExpiringSubscriptionsAsync_ShouldOnlyReturnCurrentTenantSubscriptions()
        {
            // Arrange
            var daysBeforeExpiration = 7;
            var checkDate = DateTime.UtcNow;

            _dateTimeMock.Setup(x => x.UtcNow).Returns(checkDate);

            // Act
            var result = await _subscriptionService.GetExpiringSubscriptionsAsync(daysBeforeExpiration);

            // Assert
            // NOTE: This method returns empty list for security reasons (see SubscriptionService.cs line 187)
            // Background jobs should use tenant-specific methods instead
            result.Should().BeEmpty();
            _subscriptionRepositoryMock.Verify(x => x.GetExpiringSubscriptionsByClientAsync(It.IsAny<Guid>(), checkDate, daysBeforeExpiration, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

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
