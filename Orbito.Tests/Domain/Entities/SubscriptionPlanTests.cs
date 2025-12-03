using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Domain.Entities
{
    public class SubscriptionPlanTests
    {
        private readonly TenantId _tenantId = TenantId.New();

        [Fact]
        public void Create_WithValidParameters_ShouldCreateSubscriptionPlan()
        {
            // Arrange
            var name = "Test Plan";
            var amount = 29.99m;
            var currency = "USD";
            var billingPeriodType = BillingPeriodType.Monthly;
            var description = "Test description";
            var trialDays = 14;
            var trialPeriodDays = 14;
            var featuresJson = "{\"features\":[{\"name\":\"Feature1\",\"isEnabled\":true}]}";
            var limitationsJson = "{\"limitations\":[{\"name\":\"Limit1\",\"type\":1,\"numericValue\":10}]}";
            var sortOrder = 1;

            // Act
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                name,
                amount,
                currency,
                billingPeriodType,
                description,
                trialDays,
                trialPeriodDays,
                featuresJson,
                limitationsJson,
                sortOrder);

            // Assert
            subscriptionPlan.Should().NotBeNull();
            subscriptionPlan.Id.Should().NotBe(Guid.Empty);
            subscriptionPlan.TenantId.Should().Be(_tenantId);
            subscriptionPlan.Name.Should().Be(name);
            subscriptionPlan.Description.Should().Be(description);
            subscriptionPlan.Price.Amount.Should().Be(amount);
            subscriptionPlan.Price.Currency.Code.Should().Be(currency);
            subscriptionPlan.BillingPeriod.Type.Should().Be(billingPeriodType);
            subscriptionPlan.TrialDays.Should().Be(trialDays);
            subscriptionPlan.TrialPeriodDays.Should().Be(trialPeriodDays);
            subscriptionPlan.FeaturesJson.Should().Be(featuresJson);
            subscriptionPlan.LimitationsJson.Should().Be(limitationsJson);
            subscriptionPlan.IsActive.Should().BeTrue();
            subscriptionPlan.IsPublic.Should().BeTrue();
            subscriptionPlan.SortOrder.Should().Be(sortOrder);
            subscriptionPlan.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            subscriptionPlan.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Create_WithMinimalParameters_ShouldCreateSubscriptionPlanWithDefaults()
        {
            // Arrange
            var name = "Minimal Plan";
            var amount = 0m;
            var currency = "USD";
            var billingPeriodType = BillingPeriodType.Monthly;

            // Act
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                name,
                amount,
                currency,
                billingPeriodType);

            // Assert
            subscriptionPlan.Should().NotBeNull();
            subscriptionPlan.Name.Should().Be(name);
            subscriptionPlan.Description.Should().BeNull();
            subscriptionPlan.Price.Amount.Should().Be(amount);
            subscriptionPlan.Price.Currency.Code.Should().Be(currency);
            subscriptionPlan.BillingPeriod.Type.Should().Be(billingPeriodType);
            subscriptionPlan.TrialDays.Should().Be(0);
            subscriptionPlan.TrialPeriodDays.Should().Be(0);
            subscriptionPlan.FeaturesJson.Should().BeNull();
            subscriptionPlan.LimitationsJson.Should().BeNull();
            subscriptionPlan.IsActive.Should().BeTrue();
            subscriptionPlan.IsPublic.Should().BeTrue();
            subscriptionPlan.SortOrder.Should().Be(0);
        }

        [Fact]
        public void UpdatePrice_ShouldUpdatePriceAndUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var newPrice = Money.Create(39.99m, "EUR");

            // Act
            subscriptionPlan.UpdatePrice(newPrice);

            // Assert
            subscriptionPlan.Price.Should().Be(newPrice);
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateFeatures_ShouldUpdateFeaturesJsonAndUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var newFeaturesJson = "{\"features\":[{\"name\":\"New Feature\",\"isEnabled\":true}]}";

            // Act
            subscriptionPlan.UpdateFeatures(newFeaturesJson);

            // Assert
            subscriptionPlan.FeaturesJson.Should().Be(newFeaturesJson);
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateFeatures_WithNull_ShouldSetFeaturesJsonToNull()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                featuresJson: "{\"features\":[{\"name\":\"Feature\",\"isEnabled\":true}]}");

            // Act
            subscriptionPlan.UpdateFeatures(null);

            // Assert
            subscriptionPlan.FeaturesJson.Should().BeNull();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateLimitations_ShouldUpdateLimitationsJsonAndUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var newLimitationsJson = "{\"limitations\":[{\"name\":\"New Limit\",\"type\":1,\"numericValue\":20}]}";

            // Act
            subscriptionPlan.UpdateLimitations(newLimitationsJson);

            // Assert
            subscriptionPlan.LimitationsJson.Should().Be(newLimitationsJson);
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateLimitations_WithNull_ShouldSetLimitationsJsonToNull()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                limitationsJson: "{\"limitations\":[{\"name\":\"Limit\",\"type\":1,\"numericValue\":10}]}");

            // Act
            subscriptionPlan.UpdateLimitations(null);

            // Assert
            subscriptionPlan.LimitationsJson.Should().BeNull();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateTrialPeriod_ShouldUpdateTrialPeriodDaysAndUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                trialPeriodDays: 7);

            var newTrialPeriodDays = 30;

            // Act
            subscriptionPlan.UpdateTrialPeriod(newTrialPeriodDays);

            // Assert
            subscriptionPlan.TrialPeriodDays.Should().Be(newTrialPeriodDays);
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateSortOrder_ShouldUpdateSortOrderAndUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                sortOrder: 1);

            var newSortOrder = 5;

            // Act
            subscriptionPlan.UpdateSortOrder(newSortOrder);

            // Assert
            subscriptionPlan.SortOrder.Should().Be(newSortOrder);
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrueAndUpdateUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            subscriptionPlan.Deactivate();

            // Act
            subscriptionPlan.Activate();

            // Assert
            subscriptionPlan.IsActive.Should().BeTrue();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalseAndUpdateUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Act
            subscriptionPlan.Deactivate();

            // Assert
            subscriptionPlan.IsActive.Should().BeFalse();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateVisibility_WithTrue_ShouldSetIsPublicToTrueAndUpdateUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);
            subscriptionPlan.UpdateVisibility(false);

            // Act
            subscriptionPlan.UpdateVisibility(true);

            // Assert
            subscriptionPlan.IsPublic.Should().BeTrue();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateVisibility_WithFalse_ShouldSetIsPublicToFalseAndUpdateUpdatedAt()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Act
            subscriptionPlan.UpdateVisibility(false);

            // Assert
            subscriptionPlan.IsPublic.Should().BeFalse();
            subscriptionPlan.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CanBeDeleted_WithNoSubscriptions_ShouldReturnTrue()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Act
            var canBeDeleted = subscriptionPlan.CanBeDeleted();

            // Assert
            canBeDeleted.Should().BeTrue();
        }

        [Fact]
        public void CanBeDeleted_WithOnlyInactiveSubscriptions_ShouldReturnTrue()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add cancelled subscription
            var cancelledSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                subscriptionPlan.Id,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            cancelledSubscription.Status = SubscriptionStatus.Cancelled;
            cancelledSubscription.StartDate = DateTime.UtcNow.AddMonths(-1);
            cancelledSubscription.EndDate = DateTime.UtcNow;
            subscriptionPlan.Subscriptions.Add(cancelledSubscription);

            // Act
            var canBeDeleted = subscriptionPlan.CanBeDeleted();

            // Assert
            canBeDeleted.Should().BeTrue();
        }

        [Fact]
        public void CanBeDeleted_WithActiveSubscriptions_ShouldReturnFalse()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add active subscription
            var activeSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                subscriptionPlan.Id,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            activeSubscription.Status = SubscriptionStatus.Active;
            activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscriptionPlan.Subscriptions.Add(activeSubscription);

            // Act
            var canBeDeleted = subscriptionPlan.CanBeDeleted();

            // Assert
            canBeDeleted.Should().BeFalse();
        }

        [Fact]
        public void CanBeDeleted_WithMixedSubscriptions_ShouldReturnFalse()
        {
            // Arrange
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                "Test Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            // Add cancelled subscription
            var cancelledSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                subscriptionPlan.Id,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            cancelledSubscription.Status = SubscriptionStatus.Cancelled;
            cancelledSubscription.StartDate = DateTime.UtcNow.AddMonths(-1);
            cancelledSubscription.EndDate = DateTime.UtcNow;
            subscriptionPlan.Subscriptions.Add(cancelledSubscription);

            // Add active subscription
            var activeSubscription = Subscription.Create(
                _tenantId,
                Guid.NewGuid(),
                subscriptionPlan.Id,
                Money.Create(29.99m, "USD"),
                BillingPeriod.Create(1, BillingPeriodType.Monthly));
            activeSubscription.Status = SubscriptionStatus.Active;
            activeSubscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
            subscriptionPlan.Subscriptions.Add(activeSubscription);

            // Act
            var canBeDeleted = subscriptionPlan.CanBeDeleted();

            // Assert
            canBeDeleted.Should().BeFalse();
        }

        [Theory]
        [InlineData(BillingPeriodType.Daily)]
        [InlineData(BillingPeriodType.Weekly)]
        [InlineData(BillingPeriodType.Monthly)]
        [InlineData(BillingPeriodType.Yearly)]
        public void Create_WithDifferentBillingPeriods_ShouldCreateCorrectBillingPeriod(BillingPeriodType billingPeriodType)
        {
            // Arrange
            var name = "Test Plan";
            var amount = 29.99m;
            var currency = "USD";

            // Act
            var subscriptionPlan = SubscriptionPlan.Create(
                _tenantId,
                name,
                amount,
                currency,
                billingPeriodType);

            // Assert
            subscriptionPlan.BillingPeriod.Type.Should().Be(billingPeriodType);
            subscriptionPlan.BillingPeriod.Value.Should().Be(1);
        }
    }
}
