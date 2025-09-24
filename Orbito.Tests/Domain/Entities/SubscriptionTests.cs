using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Domain.Entities
{
    public class SubscriptionTests
    {
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();
        private readonly Guid _planId = Guid.NewGuid();
        private readonly Money _price = Money.Create(29.99m, "USD");
        private readonly BillingPeriod _billingPeriod = BillingPeriod.Create(1, BillingPeriodType.Monthly);

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithValidParameters_ShouldCreateSubscription()
        {
            // Act
            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                _planId,
                _price,
                _billingPeriod);

            // Assert
            subscription.Should().NotBeNull();
            subscription.TenantId.Should().Be(_tenantId);
            subscription.ClientId.Should().Be(_clientId);
            subscription.PlanId.Should().Be(_planId);
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.CurrentPrice.Should().Be(_price);
            subscription.BillingPeriod.Should().Be(_billingPeriod);
            subscription.IsInTrial.Should().BeFalse();
            subscription.TrialEndDate.Should().BeNull();
            subscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_WithTrialDays_ShouldCreateSubscriptionWithTrial()
        {
            // Arrange
            var trialDays = 14;

            // Act
            var subscription = Subscription.Create(
                _tenantId,
                _clientId,
                _planId,
                _price,
                _billingPeriod,
                trialDays);

            // Assert
            subscription.IsInTrial.Should().BeTrue();
            subscription.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(trialDays), TimeSpan.FromSeconds(1));
            subscription.NextBillingDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(trialDays), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_WithPendingStatus_ShouldActivateSubscription()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Pending;

            // Act
            subscription.Activate();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_WithSuspendedStatus_ShouldActivateSubscription()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            // Act
            subscription.Activate();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Activate_WithActiveStatus_ShouldNotChangeStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            var originalUpdatedAt = subscription.UpdatedAt;

            // Act
            subscription.Activate();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.UpdatedAt.Should().Be(originalUpdatedAt);
        }

        [Fact]
        public void Cancel_ShouldSetCancelledStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();

            // Act
            subscription.Cancel();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
            subscription.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Suspend_WithActiveStatus_ShouldSuspendSubscription()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;

            // Act
            subscription.Suspend();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Suspended);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Suspend_WithNonActiveStatus_ShouldNotChangeStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;
            var originalUpdatedAt = subscription.UpdatedAt;

            // Act
            subscription.Suspend();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
            subscription.UpdatedAt.Should().Be(originalUpdatedAt);
        }

        [Fact]
        public void Resume_WithSuspendedStatus_ShouldResumeSubscription()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Suspended;

            // Act
            subscription.Resume();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Resume_WithNonSuspendedStatus_ShouldNotChangeStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            var originalUpdatedAt = subscription.UpdatedAt;

            // Act
            subscription.Resume();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Active);
            subscription.UpdatedAt.Should().Be(originalUpdatedAt);
        }

        [Fact]
        public void MarkAsPastDue_WithActiveStatus_ShouldSetPastDueStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;

            // Act
            subscription.MarkAsPastDue();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.PastDue);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MarkAsExpired_ShouldSetExpiredStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();

            // Act
            subscription.MarkAsExpired();

            // Assert
            subscription.Status.Should().Be(SubscriptionStatus.Expired);
            subscription.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ChangePlan_ShouldUpdatePlanAndPrice()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            var newPlanId = Guid.NewGuid();
            var newPrice = Money.Create(49.99m, "USD");

            // Act
            subscription.ChangePlan(newPlanId, newPrice);

            // Assert
            subscription.PlanId.Should().Be(newPlanId);
            subscription.CurrentPrice.Should().Be(newPrice);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateNextBillingDate_ShouldUpdateNextBillingDate()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            var originalNextBillingDate = subscription.NextBillingDate;

            // Act
            subscription.UpdateNextBillingDate();

            // Assert
            subscription.NextBillingDate.Should().BeAfter(originalNextBillingDate);
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void EndTrial_WithTrialSubscription_ShouldEndTrial()
        {
            // Arrange
            var subscription = CreateTestSubscriptionWithTrial();
            subscription.IsInTrial = true;
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(7);

            // Act
            subscription.EndTrial();

            // Assert
            subscription.IsInTrial.Should().BeFalse();
            subscription.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            subscription.NextBillingDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(1));
            subscription.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void EndTrial_WithNonTrialSubscription_ShouldNotChangeTrialStatus()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.IsInTrial = false;

            // Act
            subscription.EndTrial();

            // Assert
            subscription.IsInTrial.Should().BeFalse();
            subscription.TrialEndDate.Should().BeNull();
        }

        [Theory]
        [InlineData(SubscriptionStatus.Active, true, false)] // Trial subscriptions cannot be upgraded
        [InlineData(SubscriptionStatus.Active, false, true)]
        [InlineData(SubscriptionStatus.Suspended, true, false)]
        [InlineData(SubscriptionStatus.Cancelled, true, false)]
        [InlineData(SubscriptionStatus.PastDue, true, false)]
        public void CanBeUpgraded_ShouldReturnCorrectValue(SubscriptionStatus status, bool isInTrial, bool expected)
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = status;
            subscription.IsInTrial = isInTrial;

            // Act
            var result = subscription.CanBeUpgraded();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(SubscriptionStatus.Active, true, false)] // Trial subscriptions cannot be downgraded
        [InlineData(SubscriptionStatus.Active, false, true)]
        [InlineData(SubscriptionStatus.Suspended, true, false)]
        [InlineData(SubscriptionStatus.Cancelled, true, false)]
        [InlineData(SubscriptionStatus.PastDue, true, false)]
        public void CanBeDowngraded_ShouldReturnCorrectValue(SubscriptionStatus status, bool isInTrial, bool expected)
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = status;
            subscription.IsInTrial = isInTrial;

            // Act
            var result = subscription.CanBeDowngraded();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(SubscriptionStatus.Active, true)]
        [InlineData(SubscriptionStatus.Suspended, true)]
        [InlineData(SubscriptionStatus.Cancelled, false)]
        [InlineData(SubscriptionStatus.PastDue, false)]
        [InlineData(SubscriptionStatus.Expired, false)]
        public void CanBeCancelled_ShouldReturnCorrectValue(SubscriptionStatus status, bool expected)
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = status;

            // Act
            var result = subscription.CanBeCancelled();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(SubscriptionStatus.Active, true)]
        [InlineData(SubscriptionStatus.Suspended, false)]
        [InlineData(SubscriptionStatus.Cancelled, false)]
        [InlineData(SubscriptionStatus.PastDue, false)]
        [InlineData(SubscriptionStatus.Expired, false)]
        public void CanBeSuspended_ShouldReturnCorrectValue(SubscriptionStatus status, bool expected)
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = status;

            // Act
            var result = subscription.CanBeSuspended();

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(SubscriptionStatus.Suspended, true)]
        [InlineData(SubscriptionStatus.Active, false)]
        [InlineData(SubscriptionStatus.Cancelled, false)]
        [InlineData(SubscriptionStatus.PastDue, false)]
        [InlineData(SubscriptionStatus.Expired, false)]
        public void CanBeResumed_ShouldReturnCorrectValue(SubscriptionStatus status, bool expected)
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = status;

            // Act
            var result = subscription.CanBeResumed();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsExpiring_WithActiveSubscriptionAndExpiringDate_ShouldReturnTrue()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(5);
            var checkDate = DateTime.UtcNow;

            // Act
            var result = subscription.IsExpiring(checkDate, 7);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsExpiring_WithNonActiveSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(5);
            var checkDate = DateTime.UtcNow;

            // Act
            var result = subscription.IsExpiring(checkDate, 7);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WithActiveSubscriptionAndExpiredDate_ShouldReturnTrue()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Active;
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
            var checkDate = DateTime.UtcNow;

            // Act
            var result = subscription.IsExpired(checkDate);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsExpired_WithNonActiveSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = CreateTestSubscription();
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.NextBillingDate = DateTime.UtcNow.AddDays(-1);
            var checkDate = DateTime.UtcNow;

            // Act
            var result = subscription.IsExpired(checkDate);

            // Assert
            result.Should().BeFalse();
        }

        private Subscription CreateTestSubscription()
        {
            return Subscription.Create(
                _tenantId,
                _clientId,
                _planId,
                _price,
                _billingPeriod);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithEmptyGuidClientId_ShouldCreateSubscription()
        {
            // Act
            var subscription = Subscription.Create(_tenantId, Guid.Empty, _planId, _price, _billingPeriod);
            
            // Assert
            subscription.Should().NotBeNull();
            subscription.ClientId.Should().Be(Guid.Empty);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithEmptyGuidPlanId_ShouldCreateSubscription()
        {
            // Act
            var subscription = Subscription.Create(_tenantId, _clientId, Guid.Empty, _price, _billingPeriod);
            
            // Assert
            subscription.Should().NotBeNull();
            subscription.PlanId.Should().Be(Guid.Empty);
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithNullBillingPeriod_ShouldThrowNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => 
                Subscription.Create(_tenantId, _clientId, _planId, _price, null!));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_WithZeroPrice_ShouldCreateSubscription()
        {
            // Arrange
            var zeroPrice = Money.Create(0m, "USD");

            // Act
            var subscription = Subscription.Create(_tenantId, _clientId, _planId, zeroPrice, _billingPeriod);

            // Assert
            subscription.Should().NotBeNull();
            subscription.CurrentPrice.Should().Be(zeroPrice);
        }

        private Subscription CreateTestSubscriptionWithTrial()
        {
            return Subscription.Create(
                _tenantId,
                _clientId,
                _planId,
                _price,
                _billingPeriod,
                14);
        }
    }
}
