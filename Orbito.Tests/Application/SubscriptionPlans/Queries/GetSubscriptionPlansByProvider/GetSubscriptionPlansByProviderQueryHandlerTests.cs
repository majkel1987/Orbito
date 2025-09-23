using FluentAssertions;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public class GetSubscriptionPlansByProviderQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetSubscriptionPlansByProviderQueryHandler _handler;
        private readonly TenantId _tenantId;

        public GetSubscriptionPlansByProviderQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _handler = new GetSubscriptionPlansByProviderQueryHandler(_unitOfWorkMock.Object, _tenantContextMock.Object);
            _tenantId = TenantId.New();

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidQuery_ShouldReturnSubscriptionPlansList()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = null
            };

            var subscriptionPlans = new List<SubscriptionPlan>
            {
                SubscriptionPlan.Create(_tenantId, "Plan 1", 29.99m, "USD", BillingPeriodType.Monthly),
                SubscriptionPlan.Create(_tenantId, "Plan 2", 99.99m, "USD", BillingPeriodType.Yearly)
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(1);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();

            result.Items[0].Name.Should().Be("Plan 1");
            result.Items[0].Amount.Should().Be(29.99m);
            result.Items[0].Currency.Should().Be("USD");
            result.Items[0].BillingPeriod.Should().Be("1 Monthly");

            result.Items[1].Name.Should().Be("Plan 2");
            result.Items[1].Amount.Should().Be(99.99m);
            result.Items[1].Currency.Should().Be("USD");
            result.Items[1].BillingPeriod.Should().Be("1 Yearly");

            _subscriptionPlanRepositoryMock.Verify(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()), Times.Once);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithActiveOnlyFilter_ShouldReturnOnlyActivePlans()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = true,
                PublicOnly = false,
                SearchTerm = null
            };

            var activePlan = SubscriptionPlan.Create(_tenantId, "Active Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            activePlan.Activate();

            var inactivePlan = SubscriptionPlan.Create(_tenantId, "Inactive Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            inactivePlan.Deactivate();

            var subscriptionPlans = new List<SubscriptionPlan> { activePlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("Active Plan");
            result.Items[0].IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithPublicOnlyFilter_ShouldReturnOnlyPublicPlans()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = true,
                SearchTerm = null
            };

            var publicPlan = SubscriptionPlan.Create(_tenantId, "Public Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            publicPlan.UpdateVisibility(true);

            var privatePlan = SubscriptionPlan.Create(_tenantId, "Private Plan", 49.99m, "USD", BillingPeriodType.Monthly);
            privatePlan.UpdateVisibility(false);

            var subscriptionPlans = new List<SubscriptionPlan> { publicPlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("Public Plan");
            result.Items[0].IsPublic.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithSearchTerm_ShouldFilterPlansByName()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = "Premium"
            };

            var premiumPlan = SubscriptionPlan.Create(_tenantId, "Premium Plan", 99.99m, "USD", BillingPeriodType.Yearly);
            var basicPlan = SubscriptionPlan.Create(_tenantId, "Basic Plan", 19.99m, "USD", BillingPeriodType.Monthly);

            var subscriptionPlans = new List<SubscriptionPlan> { premiumPlan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("Premium Plan");
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldReturnCorrectPaginationInfo()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 2,
                PageSize = 5,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = null
            };

            var subscriptionPlans = new List<SubscriptionPlan>
            {
                SubscriptionPlan.Create(_tenantId, "Plan 1", 29.99m, "USD", BillingPeriodType.Monthly),
                SubscriptionPlan.Create(_tenantId, "Plan 2", 49.99m, "USD", BillingPeriodType.Monthly)
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(12); // Total count for pagination calculation

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(12);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalPages.Should().Be(3); // Math.Ceiling(12/5) = 3
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithEmptyResults_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = "NonExistent"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubscriptionPlan>());

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(0);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithPlansWithSubscriptions_ShouldCalculateSubscriptionCounts()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = null
            };

            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            var subscriptionPlans = new List<SubscriptionPlan> { plan };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _subscriptionPlanRepositoryMock.Setup(x => x.GetAllAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptionPlans);

            _subscriptionPlanRepositoryMock.Setup(x => x.GetCountAsync(
                query.SearchTerm,
                query.ActiveOnly,
                query.PublicOnly,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].ActiveSubscriptionsCount.Should().Be(0); // No subscriptions added in this test
            result.Items[0].TotalSubscriptionsCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldThrowException()
        {
            // Arrange
            var query = new GetSubscriptionPlansByProviderQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false,
                PublicOnly = false,
                SearchTerm = null
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));

            exception.Message.Should().Be("Tenant context is required to get subscription plans");
        }
    }
}
