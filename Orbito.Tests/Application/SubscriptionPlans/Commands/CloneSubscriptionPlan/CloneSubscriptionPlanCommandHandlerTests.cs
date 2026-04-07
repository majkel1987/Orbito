using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan
{
    public class CloneSubscriptionPlanCommandHandlerTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CloneSubscriptionPlanCommandHandler>> _mockLogger;
        private readonly CloneSubscriptionPlanCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CloneSubscriptionPlanCommandHandlerTests()
        {
            _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<CloneSubscriptionPlanCommandHandler>>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _unitOfWorkMock.Setup(x => x.SubscriptionPlans).Returns(_subscriptionPlanRepositoryMock.Object);

            _handler = new CloneSubscriptionPlanCommandHandler(
                _unitOfWorkMock.Object,
                _tenantContextMock.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCloneSubscriptionPlan()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Original description",
                14,
                14,
                "{\"features\":[{\"name\":\"Feature1\",\"isEnabled\":true}]}",
                "{\"limitations\":[{\"name\":\"Limit1\",\"type\":1,\"numericValue\":10}]}",
                1);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Cloned Plan",
                NewDescription = "Cloned description",
                NewAmount = 39.99m,
                NewCurrency = "EUR",
                IsActive = true,
                IsPublic = false,
                NewSortOrder = 2
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                command.NewAmount!.Value,
                command.NewCurrency!,
                originalPlan.BillingPeriod.Type,
                command.NewDescription,
                originalPlan.TrialDays,
                originalPlan.TrialPeriodDays,
                originalPlan.FeaturesJson,
                originalPlan.LimitationsJson,
                command.NewSortOrder!.Value);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().NotBeEmpty();
            result.Value.Name.Should().Be(command.NewName);
            result.Value.Description.Should().Be(command.NewDescription);
            result.Value.Amount.Should().Be(command.NewAmount.Value);
            result.Value.Currency.Should().Be(command.NewCurrency);
            result.Value.BillingPeriod.Should().Be(originalPlan.BillingPeriod.ToString());
            result.Value.TrialPeriodDays.Should().Be(originalPlan.TrialPeriodDays);
            result.Value.IsActive.Should().Be(command.IsActive);
            result.Value.IsPublic.Should().Be(command.IsPublic);
            result.Value.SortOrder.Should().Be(command.NewSortOrder.Value);
            result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.Value.OriginalPlanId.Should().NotBeEmpty();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithMinimalCommand_ShouldCloneWithOriginalValues()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Original description",
                14,
                14,
                "{\"features\":[{\"name\":\"Feature1\",\"isEnabled\":true}]}",
                "{\"limitations\":[{\"name\":\"Limit1\",\"type\":1,\"numericValue\":10}]}",
                1);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Cloned Plan"
                // All other values should be taken from original plan
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                originalPlan.Price.Amount,
                originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type,
                originalPlan.Description,
                originalPlan.TrialDays,
                originalPlan.TrialPeriodDays,
                originalPlan.FeaturesJson,
                originalPlan.LimitationsJson,
                originalPlan.SortOrder);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(command.NewName);
            result.Value.Description.Should().Be(originalPlan.Description);
            result.Value.Amount.Should().Be(originalPlan.Price.Amount);
            result.Value.Currency.Should().Be(originalPlan.Price.Currency);
            result.Value.TrialPeriodDays.Should().Be(originalPlan.TrialPeriodDays);
            result.Value.SortOrder.Should().Be(originalPlan.SortOrder);
            result.Value.IsActive.Should().BeTrue(); // Default value
            result.Value.IsPublic.Should().BeTrue(); // Default value

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInactiveClonedPlan_ShouldCreateInactivePlan()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Inactive Cloned Plan",
                IsActive = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                originalPlan.Price.Amount,
                originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type);
            clonedPlan.Deactivate();

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IsActive.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPrivateClonedPlan_ShouldCreatePrivatePlan()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Private Cloned Plan",
                IsPublic = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                originalPlan.Price.Amount,
                originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type);
            clonedPlan.UpdateVisibility(false);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.IsPublic.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithPartialOverrides_ShouldCloneWithMixedValues()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                "Original description",
                14,
                14,
                null,
                null,
                1);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Partially Overridden Plan",
                NewAmount = 49.99m,
                // NewCurrency not provided - should use original
                // NewDescription not provided - should use original
                // NewSortOrder not provided - should use original
                IsActive = false,
                IsPublic = false
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                command.NewAmount!.Value,
                originalPlan.Price.Currency, // Use original currency
                originalPlan.BillingPeriod.Type,
                originalPlan.Description, // Use original description
                originalPlan.TrialDays,
                originalPlan.TrialPeriodDays,
                originalPlan.FeaturesJson,
                originalPlan.LimitationsJson,
                originalPlan.SortOrder); // Use original sort order

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(command.NewName);
            result.Value.Amount.Should().Be(command.NewAmount.Value);
            result.Value.Currency.Should().Be(originalPlan.Price.Currency);
            result.Value.Description.Should().Be(originalPlan.Description);
            result.Value.SortOrder.Should().Be(originalPlan.SortOrder);
            result.Value.IsActive.Should().BeFalse();
            result.Value.IsPublic.Should().BeFalse();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithFeaturesAndLimitations_ShouldCloneJsonProperties()
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var featuresJson = "{\"features\":[{\"name\":\"Advanced Analytics\",\"description\":\"Detailed reports\",\"isEnabled\":true}]}";
            var limitationsJson = "{\"limitations\":[{\"name\":\"Storage\",\"type\":1,\"numericValue\":1000,\"description\":\"GB of storage\"}]}";

            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                BillingPeriodType.Monthly,
                featuresJson: featuresJson,
                limitationsJson: limitationsJson);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Cloned Plan with Features"
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                originalPlan.Price.Amount,
                originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type,
                featuresJson: featuresJson,
                limitationsJson: limitationsJson);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(command.NewName);
            result.Value.OriginalPlanId.Should().NotBeEmpty();

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new CloneSubscriptionPlanCommand
            {
                Id = Guid.NewGuid(),
                NewName = "Test Plan"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentPlan_ShouldReturnFailure()
        {
            // Arrange
            var planId = Guid.NewGuid();
            var command = new CloneSubscriptionPlanCommand
            {
                Id = planId,
                NewName = "Non-existent Plan"
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.SubscriptionPlan.NotFound);

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(planId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(BillingPeriodType.Daily)]
        [InlineData(BillingPeriodType.Weekly)]
        [InlineData(BillingPeriodType.Monthly)]
        [InlineData(BillingPeriodType.Yearly)]
        public async Task Handle_WithDifferentBillingPeriods_ShouldCloneCorrectBillingPeriod(BillingPeriodType billingPeriodType)
        {
            // Arrange
            var originalPlanId = Guid.NewGuid();
            var originalPlan = SubscriptionPlan.Create(
                _tenantId,
                "Original Plan",
                29.99m,
                "USD",
                billingPeriodType);

            var command = new CloneSubscriptionPlanCommand
            {
                Id = originalPlanId,
                NewName = "Cloned Plan"
            };

            _subscriptionPlanRepositoryMock.Setup(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPlan);

            var clonedPlan = SubscriptionPlan.Create(
                _tenantId,
                command.NewName,
                originalPlan.Price.Amount,
                originalPlan.Price.Currency,
                originalPlan.BillingPeriod.Type);

            _subscriptionPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clonedPlan);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.BillingPeriod.Should().Be(originalPlan.BillingPeriod.ToString());

            _subscriptionPlanRepositoryMock.Verify(x => x.GetByIdAsync(originalPlanId, It.IsAny<CancellationToken>()), Times.Once);
            _subscriptionPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SubscriptionPlan>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
