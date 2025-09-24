using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Queries.GetSubscriptionsByClient
{
    public class GetSubscriptionsByClientQueryHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ILogger<GetSubscriptionsByClientQueryHandler>> _loggerMock;
        private readonly GetSubscriptionsByClientQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetSubscriptionsByClientQueryHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _loggerMock = new Mock<ILogger<GetSubscriptionsByClientQueryHandler>>();

            _handler = new GetSubscriptionsByClientQueryHandler(
                _subscriptionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidClientId_ShouldReturnSubscriptions()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false
            };

            var subscriptions = CreateTestSubscriptions(clientId);
            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(3);
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(1);

            _subscriptionRepositoryMock.Verify(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithActiveOnlyFilter_ShouldReturnOnlyActiveSubscriptions()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = true
            };

            var subscriptions = CreateTestSubscriptions(clientId);
            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Subscriptions.Should().OnlyContain(s => s.Status == SubscriptionStatus.Active);

            _subscriptionRepositoryMock.Verify(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 2,
                PageSize = 2,
                ActiveOnly = false
            };

            var subscriptions = CreateTestSubscriptions(clientId);
            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(1); // Only 1 subscription on page 2
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.TotalPages.Should().Be(2);

            _subscriptionRepositoryMock.Verify(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNoSubscriptions_ShouldReturnEmptyResult()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Subscription>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(0);

            _subscriptionRepositoryMock.Verify(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithTrialSubscriptions_ShouldReturnCorrectTrialInformation()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false
            };

            var subscriptions = new List<Subscription>
            {
                CreateTestSubscriptionWithTrial(clientId)
            };
            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(1);
            var subscription = result.Subscriptions.First();
            subscription.IsInTrial.Should().BeTrue();
            subscription.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_WithCancelledSubscriptions_ShouldReturnCorrectCancellationInformation()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 10,
                ActiveOnly = false
            };

            var subscription = CreateTestSubscription(clientId);
            subscription.Cancel();
            var subscriptions = new List<Subscription> { subscription };

            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(1);
            var subscriptionDto = result.Subscriptions.First();
            subscriptionDto.Status.Should().Be(SubscriptionStatus.Cancelled);
            subscriptionDto.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_WithLargePageSize_ShouldReturnAllSubscriptions()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetSubscriptionsByClientQuery
            {
                ClientId = clientId,
                PageNumber = 1,
                PageSize = 100,
                ActiveOnly = false
            };

            var subscriptions = CreateTestSubscriptions(clientId);
            _subscriptionRepositoryMock.Setup(x => x.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriptions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Subscriptions.Should().HaveCount(3);
            result.TotalCount.Should().Be(3);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(100);
            result.TotalPages.Should().Be(1);
        }

        private List<Subscription> CreateTestSubscriptions(Guid clientId)
        {
            var subscriptions = new List<Subscription>();

            // Active subscription
            var activeSubscription = CreateTestSubscription(clientId);
            activeSubscription.Status = SubscriptionStatus.Active;
            subscriptions.Add(activeSubscription);

            // Cancelled subscription
            var cancelledSubscription = CreateTestSubscription(clientId);
            cancelledSubscription.Status = SubscriptionStatus.Cancelled;
            cancelledSubscription.Cancel();
            subscriptions.Add(cancelledSubscription);

            // Expired subscription
            var expiredSubscription = CreateTestSubscription(clientId);
            expiredSubscription.Status = SubscriptionStatus.Expired;
            expiredSubscription.MarkAsExpired();
            subscriptions.Add(expiredSubscription);

            return subscriptions;
        }

        private Subscription CreateTestSubscription(Guid clientId)
        {
            return Subscription.Create(
                _tenantId,
                clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
        }

        private Subscription CreateTestSubscriptionWithTrial(Guid clientId)
        {
            return Subscription.Create(
                _tenantId,
                clientId,
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                14);
        }
    }
}
