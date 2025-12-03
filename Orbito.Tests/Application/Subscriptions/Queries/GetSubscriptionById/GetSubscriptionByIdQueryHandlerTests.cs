using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Subscriptions.Queries.GetSubscriptionById;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Subscriptions.Queries.GetSubscriptionById
{
    public class GetSubscriptionByIdQueryHandlerTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<GetSubscriptionByIdQueryHandler>> _loggerMock;
        private readonly GetSubscriptionByIdQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetSubscriptionByIdQueryHandlerTests()
        {
            _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<GetSubscriptionByIdQueryHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new GetSubscriptionByIdQueryHandler(
                _subscriptionRepositoryMock.Object,
                _tenantContextMock.Object,
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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.ClientId.Should().Be(subscription.ClientId);
            result.Value.PlanId.Should().Be(subscription.PlanId);
            result.Value.Status.Should().Be(subscription.Status.ToString());
            result.Value.Amount.Should().Be(subscription.CurrentPrice.Amount);
            result.Value.Currency.Should().Be(subscription.CurrentPrice.Currency);
            result.Value.BillingPeriodValue.Should().Be(subscription.BillingPeriod.Value);
            result.Value.BillingPeriodType.Should().Be(subscription.BillingPeriod.Type.ToString());
            result.Value.StartDate.Should().Be(subscription.StartDate);
            result.Value.EndDate.Should().Be(subscription.EndDate);
            result.Value.NextBillingDate.Should().Be(subscription.NextBillingDate);
            result.Value.IsInTrial.Should().Be(subscription.IsInTrial);
            result.Value.TrialEndDate.Should().Be(subscription.TrialEndDate);
            result.Value.CreatedAt.Should().Be(subscription.CreatedAt);
            result.Value.CancelledAt.Should().Be(subscription.CancelledAt);
            result.Value.UpdatedAt.Should().Be(subscription.UpdatedAt);

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(subscriptionId);
            result.Value.ClientId.Should().Be(subscription.ClientId);
            result.Value.PlanId.Should().Be(subscription.PlanId);
            result.Value.Status.Should().Be(subscription.Status.ToString());
            result.Value.ClientCompanyName.Should().Be(subscription.Client?.CompanyName);
            result.Value.ClientEmail.Should().Be(subscription.Client?.DirectEmail);
            result.Value.ClientFirstName.Should().Be(subscription.Client?.DirectFirstName);
            result.Value.ClientLastName.Should().Be(subscription.Client?.DirectLastName);
            result.Value.PlanName.Should().Be(subscription.Plan?.Name);
            result.Value.PlanDescription.Should().Be(subscription.Plan?.Description);
            result.Value.PaymentCount.Should().Be(subscription.Payments?.Count ?? 0);
            result.Value.TotalPaid.Should().Be(subscription.Payments?.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount.Amount) ?? 0);
            result.Value.LastPaymentDate.Should().Be(subscription.Payments?.Where(p => p.Status == PaymentStatus.Completed).Max(p => p.ProcessedAt));

            _subscriptionRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentSubscription_ShouldReturnFailure()
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
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Subscription.NotFound);

            _subscriptionRepositoryMock.Verify(x => x.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithIncludeDetailsAndNonExistentSubscription_ShouldReturnFailure()
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
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Subscription.NotFound);

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IsInTrial.Should().BeTrue();
            result.Value.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(1));
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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Status.Should().Be(SubscriptionStatus.Cancelled.ToString());
            result.Value.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
