using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Queries.GetSubscriptionById;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Queries.GetSubscriptionById
{
    public class GetSubscriptionByIdQueryHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ILogger<GetSubscriptionByIdQueryHandler>> _loggerMock;
        private readonly GetSubscriptionByIdQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetSubscriptionByIdQueryHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _loggerMock = new Mock<ILogger<GetSubscriptionByIdQueryHandler>>();

            _handler = new GetSubscriptionByIdQueryHandler(
                _subscriptionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithExistingSubscription_ShouldReturnSubscriptionDetails()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = false
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(subscriptionId);
            result.ClientId.Should().Be(subscription.ClientId);
            result.PlanId.Should().Be(subscription.PlanId);
            result.Status.Should().Be(subscription.Status);
            result.CurrentPrice.Should().Be(subscription.CurrentPrice.Amount);
            result.Currency.Should().Be(subscription.CurrentPrice.Currency);
            result.BillingPeriod.Should().Be(subscription.BillingPeriod.ToString());
            result.StartDate.Should().Be(subscription.StartDate);
            result.EndDate.Should().Be(subscription.EndDate);
            result.NextBillingDate.Should().Be(subscription.NextBillingDate);
            result.IsInTrial.Should().Be(subscription.IsInTrial);
            result.TrialEndDate.Should().Be(subscription.TrialEndDate);
            result.CreatedAt.Should().Be(subscription.CreatedAt);
            result.CancelledAt.Should().Be(subscription.CancelledAt);
            result.UpdatedAt.Should().Be(subscription.UpdatedAt);

            _subscriptionRepositoryMock.Verify(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithIncludeDetails_ShouldReturnSubscriptionWithDetails()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = true
            };

            var subscription = CreateTestSubscriptionWithDetails();
            subscription.Id = subscriptionId;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(subscriptionId);
            result.ClientId.Should().Be(subscription.ClientId);
            result.PlanId.Should().Be(subscription.PlanId);
            result.Status.Should().Be(subscription.Status);
            result.ClientCompanyName.Should().Be(subscription.Client?.CompanyName);
            result.ClientEmail.Should().Be(subscription.Client?.DirectEmail);
            result.ClientFirstName.Should().Be(subscription.Client?.DirectFirstName);
            result.ClientLastName.Should().Be(subscription.Client?.DirectLastName);
            result.PlanName.Should().Be(subscription.Plan?.Name);
            result.PlanDescription.Should().Be(subscription.Plan?.Description);
            result.PaymentCount.Should().Be(subscription.Payments?.Count ?? 0);
            result.TotalPaid.Should().Be(subscription.Payments?.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount.Amount) ?? 0);
            result.LastPaymentDate.Should().Be(subscription.Payments?.Where(p => p.Status == PaymentStatus.Completed).Max(p => p.ProcessedAt));

            _subscriptionRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnNull()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = false
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _subscriptionRepositoryMock.Verify(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithIncludeDetailsAndNonExistentSubscription_ShouldReturnNull()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = true
            };

            _subscriptionRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Subscription?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _subscriptionRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithTrialSubscription_ShouldReturnCorrectTrialInformation()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = false
            };

            var subscription = CreateTestSubscriptionWithTrial();
            subscription.Id = subscriptionId;

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.IsInTrial.Should().BeTrue();
            result.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_WithCancelledSubscription_ShouldReturnCorrectCancellationInformation()
        {
            // Arrange
            var subscriptionId = Guid.NewGuid();
            var query = new GetSubscriptionByIdQuery
            {
                SubscriptionId = subscriptionId,
                IncludeDetails = false
            };

            var subscription = CreateTestSubscription();
            subscription.Id = subscriptionId;
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.Cancel();

            _subscriptionRepositoryMock.Setup(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(SubscriptionStatus.Cancelled);
            result.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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

        private Subscription CreateTestSubscriptionWithTrial()
        {
            return Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly),
                14);
        }

        private Subscription CreateTestSubscriptionWithDetails()
        {
            var subscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));

            // Mock client details
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.UpdateDirectInfo("test@example.com", "John", "Doe");
            subscription.Client = client;

            // Mock plan details
            var plan = SubscriptionPlan.Create(_tenantId, "Test Plan", 29.99m, "USD", BillingPeriodType.Monthly);
            // plan.UpdateDescription("Test plan description"); // This method doesn't exist
            subscription.Plan = plan;

            // Mock payments
            var payment = Payment.Create(
                _tenantId,
                subscription.Id,
                subscription.ClientId,
                Money.Create(29.99m, "USD"),
                "external-payment-id");
            payment.MarkAsCompleted();
            subscription.Payments = [payment];

            return subscription;
        }
    }
}
