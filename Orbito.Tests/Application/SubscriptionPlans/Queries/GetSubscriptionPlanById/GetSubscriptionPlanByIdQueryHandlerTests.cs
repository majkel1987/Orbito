using FluentAssertions;
using Moq;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Queries.GetSubscriptionPlanById
{
    public class GetSubscriptionPlanByIdQueryHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly GetSubscriptionPlanByIdQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetSubscriptionPlanByIdQueryHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new GetSubscriptionPlanByIdQueryHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidId_ShouldReturnSubscriptionPlanDto()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Test description",
                14,
                14,
                "{\"features\":[{\"name\":\"Feature1\",\"isEnabled\":true}]}",
                "{\"limitations\":[{\"name\":\"Limit1\",\"type\":1,\"numericValue\":10}]}",
                1);

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().NotBeEmpty();
            result.Name.Should().Be(subscriptionPlan.Name);
            result.Description.Should().Be(subscriptionPlan.Description);
            result.Amount.Should().Be(subscriptionPlan.Price.Amount);
            result.Currency.Should().Be(subscriptionPlan.Price.Currency);
            result.BillingPeriod.Should().Be(subscriptionPlan.BillingPeriod.ToString());
            result.TrialDays.Should().Be(subscriptionPlan.TrialDays);
            result.TrialPeriodDays.Should().Be(subscriptionPlan.TrialPeriodDays);
            result.FeaturesJson.Should().Be(subscriptionPlan.FeaturesJson);
            result.LimitationsJson.Should().Be(subscriptionPlan.LimitationsJson);
            result.IsActive.Should().Be(subscriptionPlan.IsActive);
            result.IsPublic.Should().Be(subscriptionPlan.IsPublic);
            result.SortOrder.Should().Be(subscriptionPlan.SortOrder);
            result.CreatedAt.Should().Be(subscriptionPlan.CreatedAt);
            result.UpdatedAt.Should().Be(subscriptionPlan.UpdatedAt);
            result.ActiveSubscriptionsCount.Should().Be(0);
            result.TotalSubscriptionsCount.Should().Be(0);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPlanWithActiveSubscriptions_ShouldReturnCorrectCounts()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Plan with Subscriptions",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add active subscription
            var activeSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            activeSubscription.Status = SubscriptionStatus.Active;
            activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            // Add cancelled subscription
            var cancelledSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                planId,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            cancelledSubscription.Status = SubscriptionStatus.Cancelled;
            cancelledSubscription.StartDate = DateTime.UtcNow.AddMonths(-1);
            cancelledSubscription.EndDate = DateTime.UtcNow;

            subscriptionPlan.Subscriptions.Add(activeSubscription);
            subscriptionPlan.Subscriptions.Add(cancelledSubscription);

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ActiveSubscriptionsCount.Should().Be(1);
            result.TotalSubscriptionsCount.Should().Be(2);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPlanWithMultipleActiveSubscriptions_ShouldReturnCorrectActiveCount()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Plan with Multiple Active Subscriptions",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add multiple active subscriptions
            for (int i = 0; i < 3; i++)
            {
                var activeSubscription = Subscription.Create(
                    _tenantId,
                    Guid.NewGuid(),
                    planId,
                    Money.Create(29.99m, "USD"),
                    BillingPeriod.Create(1, BillingPeriodType.Monthly));
                activeSubscription.Status = SubscriptionStatus.Active;
                activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
                subscriptionPlan.Subscriptions.Add(activeSubscription);
            }

            // Add some inactive subscriptions
            for (int i = 0; i < 2; i++)
            {
                var inactiveSubscription = Subscription.Create(
                    _tenantId,
                    Guid.NewGuid(),
                    planId,
                    Money.Create(29.99m, "USD"),
                    BillingPeriod.Create(1, BillingPeriodType.Monthly));
                inactiveSubscription.Status = SubscriptionStatus.Cancelled;
                inactiveSubscription.StartDate = DateTime.UtcNow.AddMonths(-1);
                inactiveSubscription.EndDate = DateTime.UtcNow;
                subscriptionPlan.Subscriptions.Add(inactiveSubscription);
            }

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ActiveSubscriptionsCount.Should().Be(3);
            result.TotalSubscriptionsCount.Should().Be(5);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentPlan_ShouldReturnNull()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldThrowException()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = Guid.NewGuid()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _handler.Handle(query, CancellationToken.None));

            exception.Message.Should().Be("Tenant context is required to get subscription plan");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(query, CancellationToken.None));

            exception.Message.Should().Be("Database error");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithMinimalPlan_ShouldReturnDtoWithDefaultValues()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Minimal Plan",
                0m,
                "USD",
                BillingPeriodType.Monthly);

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Minimal Plan");
            result.Description.Should().BeNull();
            result.Amount.Should().Be(0m);
            result.Currency.Should().Be("USD");
            result.TrialDays.Should().Be(0);
            result.TrialPeriodDays.Should().Be(0);
            result.FeaturesJson.Should().BeNull();
            result.LimitationsJson.Should().BeNull();
            result.IsActive.Should().BeTrue();
            result.IsPublic.Should().BeTrue();
            result.SortOrder.Should().Be(0);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(BillingPeriodType.Daily, "1 Daily")]
        [InlineData(BillingPeriodType.Weekly, "1 Weekly")]
        [InlineData(BillingPeriodType.Monthly, "1 Monthly")]
        [InlineData(BillingPeriodType.Yearly, "1 Yearly")]
        public async Task Handle_WithDifferentBillingPeriods_ShouldReturnCorrectBillingPeriod(BillingPeriodType billingPeriodType, string expectedBillingPeriod)
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                billingPeriodType);

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.BillingPeriod.Should().Be(expectedBillingPeriod);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithUpdatedPlan_ShouldReturnUpdatedAt()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Simulate update
            subscriptionPlan.UpdatePrice(Money.Create(39.99m, "USD"));

            var query = new GetSubscriptionPlanByIdQuery
            {
                Id = planId
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlan);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
