using FluentAssertions;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Queries.GetActiveSubscriptionPlans
{
    public class GetActiveSubscriptionPlansQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetActiveSubscriptionPlansQueryHandler _handler;
        private readonly TenantId _tenantId;

        public GetActiveSubscriptionPlansQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _handler = new GetActiveSubscriptionPlansQueryHandler(_unitOfWorkMock.Object, _tenantContextMock.Object);
            _tenantId = TenantId.New();

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidQuery_ShouldReturnActiveSubscriptionPlans()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            var activePlan1 = SubscriptionPlan.Create(_tenantId, "Active Plan 1", 29.99m, "USD", BillingPeriodType.Monthly);
            activePlan1.Activate();
            activePlan1.UpdateVisibility(true);

            var activePlan2 = SubscriptionPlan.Create(_tenantId, "Active Plan 2", 99.99m, "USD", BillingPeriodType.Yearly);
            activePlan2.Activate();
            activePlan2.UpdateVisibility(true);

            var subscriptionPlans = new List<SubscriptionPlan> { activePlan1, activePlan2 };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);

            result.Plans[0].Name.Should().Be("Active Plan 1");
            result.Plans[0].Amount.Should().Be(29.99m);
            result.Plans[0].Currency.Should().Be("USD");
            result.Plans[0].BillingPeriod.Should().Be("1 Monthly");

            result.Plans[1].Name.Should().Be("Active Plan 2");
            result.Plans[1].Amount.Should().Be(99.99m);
            result.Plans[1].Currency.Should().Be("USD");
            result.Plans[1].BillingPeriod.Should().Be("1 Yearly");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPublicOnlyTrue_ShouldReturnOnlyPublicPlans()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            var publicPlan = SubscriptionPlan.Create(_tenantId, "Public Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            publicPlan.Activate();
            publicPlan.UpdateVisibility(true);

            var privatePlan = SubscriptionPlan.Create(_tenantId, "Private Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            privatePlan.Activate();
            privatePlan.UpdateVisibility(false);

            var subscriptionPlans = new List<SubscriptionPlan> { publicPlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(1);
            result.Plans[0].Name.Should().Be("Public Plan");
        }

        [Fact]
        public async Task Handle_WithPublicOnlyFalse_ShouldReturnAllActivePlans()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = false,
                Limit = null
            };

            var publicPlan = SubscriptionPlan.Create(_tenantId, "Public Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            publicPlan.Activate();
            publicPlan.UpdateVisibility(true);

            var privatePlan = SubscriptionPlan.Create(_tenantId, "Private Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            privatePlan.Activate();
            privatePlan.UpdateVisibility(false);

            var subscriptionPlans = new List<SubscriptionPlan> { publicPlan, privatePlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(2);
            result.Plans.Should().Contain(p => p.Name == "Public Plan");
            result.Plans.Should().Contain(p => p.Name == "Private Plan");
        }

        [Fact]
        public async Task Handle_WithLimit_ShouldReturnLimitedPlans()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = 1
            };

            var plan1 = SubscriptionPlan.Create(_tenantId, "Plan 1", 29.99m, "USD", BillingPeriodType.Monthly);
            plan1.Activate();
            plan1.UpdateVisibility(true);

            var plan2 = SubscriptionPlan.Create(_tenantId, "Plan 2", 49.99m, "USD", BillingPeriodType.Monthly);
            plan2.Activate();
            plan2.UpdateVisibility(true);

            var subscriptionPlans = new List<SubscriptionPlan> { plan1 };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(1);
            result.Plans[0].Name.Should().Be("Plan 1");
        }

        [Fact]
        public async Task Handle_WithPlansWithFeaturesAndLimitations_ShouldIncludeJsonProperties()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            var features = PlanFeatures.CreateFromJson("{\"feature1\":\"value1\",\"feature2\":\"value2\"}");
            var limitations = PlanLimitations.CreateFromJson("{\"limit1\":100,\"limit2\":\"unlimited\"}");

            var plan = SubscriptionPlan.Create(_tenantId, "Feature Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();
            plan.UpdateVisibility(true);
            plan.UpdateFeatures(features.ToJson());
            plan.UpdateLimitations(limitations.ToJson());

            var subscriptionPlans = new List<SubscriptionPlan> { plan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(1);
            result.Plans[0].FeaturesJson.Should().NotBeNullOrEmpty();
            result.Plans[0].LimitationsJson.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_WithPlansWithSubscriptions_ShouldCalculateActiveSubscriptionCount()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            plan.Activate();
            plan.UpdateVisibility(true);

            var subscriptionPlans = new List<SubscriptionPlan> { plan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(1);
            result.Plans[0].ActiveSubscriptionsCount.Should().Be(0); // No subscriptions added in this test
        }

        [Fact]
        public async Task Handle_WithEmptyResults_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubscriptionPlan>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithDifferentBillingPeriods_ShouldReturnCorrectBillingPeriodStrings()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            var dailyPlan = SubscriptionPlan.Create(_tenantId, "Daily Plan", 9.99m, "USD", BillingPeriodType.Daily);
            dailyPlan.Activate();
            dailyPlan.UpdateVisibility(true);

            var weeklyPlan = SubscriptionPlan.Create(_tenantId, "Weekly Plan", 19.99m, "USD", BillingPeriodType.Weekly);
            weeklyPlan.Activate();
            weeklyPlan.UpdateVisibility(true);

            var monthlyPlan = SubscriptionPlan.Create(_tenantId, "Monthly Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            monthlyPlan.Activate();
            monthlyPlan.UpdateVisibility(true);

            var yearlyPlan = SubscriptionPlan.Create(_tenantId, "Yearly Plan", 299.99m, "USD", BillingPeriodType.Yearly);
            yearlyPlan.Activate();
            yearlyPlan.UpdateVisibility(true);

            var subscriptionPlans = new List<SubscriptionPlan> { dailyPlan, weeklyPlan, monthlyPlan, yearlyPlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetActivePlansAsync(
                query.PublicOnly,
                query.Limit,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Plans.Should().HaveCount(4);
            
            result.Plans.Should().Contain(p => p.BillingPeriod == "1 Daily");
            result.Plans.Should().Contain(p => p.BillingPeriod == "1 Weekly");
            result.Plans.Should().Contain(p => p.BillingPeriod == "1 Monthly");
            result.Plans.Should().Contain(p => p.BillingPeriod == "1 Yearly");
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldThrowException()
        {
            // Arrange
            var query = new GetActiveSubscriptionPlansQuery
            {
                PublicOnly = true,
                Limit = null
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));

            exception.Message.Should().Be("Tenant context is required to get active subscription plans");
        }
    }
}
