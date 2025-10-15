using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Events;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers.Assertions;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Domain.Entities;

public class PaymentTests
{
    private readonly TenantId _tenantId = TenantId.New();
    private readonly Guid _subscriptionId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Money _validAmount = Money.Create(100.00m, "USD");

    #region Creation Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithValidData_ShouldCreatePayment()
    {
        // Act
        var payment = Payment.Create(_tenantId, _subscriptionId, _clientId, _validAmount);

        // Assert
        payment.Should().NotBeNull();
        payment.TenantId.Should().Be(_tenantId);
        payment.SubscriptionId.Should().Be(_subscriptionId);
        payment.ClientId.Should().Be(_clientId);
        payment.Amount.Should().Be(_validAmount);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.ProcessedAt.Should().BeNull();
        payment.FailedAt.Should().BeNull();
        payment.RefundedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithNullTenantId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => Payment.Create(null!, _subscriptionId, _clientId, _validAmount);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("tenantId");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithEmptySubscriptionId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => Payment.Create(_tenantId, Guid.Empty, _clientId, _validAmount);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Subscription ID cannot be empty*")
            .WithParameterName("subscriptionId");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithZeroAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var zeroAmount = Money.Create(0m, "USD");

        // Act & Assert
        var action = () => Payment.Create(_tenantId, _subscriptionId, _clientId, zeroAmount);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Payment amount must be greater than zero*")
            .WithParameterName("amount");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithIdempotencyKey_ShouldSetIdempotencyKey()
    {
        // Arrange
        var idempotencyKey = IdempotencyKey.Create("test-key-123");

        // Act
        var payment = Payment.Create(_tenantId, _subscriptionId, _clientId, _validAmount, idempotencyKey: idempotencyKey);

        // Assert
        payment.IdempotencyKey.Should().Be(idempotencyKey);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsProcessing_FromPending_ShouldUpdateStatus()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act
        payment.MarkAsProcessing();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.ShouldBeProcessingPayment();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCompleted_FromProcessing_ShouldSetProcessedAtAndRaiseDomainEvent()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create()
            .WithStatus(PaymentStatus.Processing)
            .Build();

        // Act
        payment.MarkAsCompleted();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.ShouldHaveDomainEvent<PaymentCompletedEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsFailed_WithReason_ShouldSetFailureReasonAndRaiseDomainEvent()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();
        var failureReason = "Insufficient funds";

        // Act
        payment.MarkAsFailed(failureReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailedAt.Should().NotBeNull();
        payment.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.FailureReason.Should().Be(failureReason);
        payment.ShouldHaveDomainEvent<PaymentFailedEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsFailed_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        var action = () => payment.MarkAsFailed("");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Failure reason cannot be empty*")
            .WithParameterName("reason");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCancelled_ShouldSetStatusAndFailedAt()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act
        payment.MarkAsCancelled();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.FailedAt.Should().NotBeNull();
        payment.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.FailureReason.Should().Be("Payment cancelled");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsRefunded_FromCompleted_ShouldSetRefundedAtAndRaiseDomainEvent()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();
        var refundReason = "Customer request";

        // Act
        payment.MarkAsRefunded(refundReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundedAt.Should().NotBeNull();
        payment.RefundedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.RefundReason.Should().Be(refundReason);
        payment.ShouldHaveDomainEvent<PaymentRefundedEvent>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsRefunded_FromPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        var action = () => payment.MarkAsRefunded("Customer request");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only completed payments can be refunded");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsPartiallyRefunded_ShouldUpdateStatus()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();
        var refundReason = "Partial refund";
        var refundedAmount = Money.Create(50.00m, "USD");

        // Act
        payment.MarkAsPartiallyRefunded(refundReason, refundedAmount);

        // Assert
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.RefundedAt.Should().NotBeNull();
        payment.RefundedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.RefundReason.Should().Be(refundReason);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RetryPayment_FromFailed_ShouldResetToPending()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.FailedPayment();

        // Act
        payment.RetryPayment();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.FailedAt.Should().BeNull();
        payment.FailureReason.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RetryPayment_FromCompleted_ShouldNotChangeStatus()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();
        var originalStatus = payment.Status;
        var originalProcessedAt = payment.ProcessedAt;

        // Act
        payment.RetryPayment();

        // Assert
        payment.Status.Should().Be(originalStatus);
        payment.ProcessedAt.Should().Be(originalProcessedAt);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeRetried_FailedWithinDaysLimit_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.RecentFailedPayment();

        // Act
        var result = payment.CanBeRetried();

        // Assert
        result.Should().BeTrue();
        payment.ShouldBeRetryable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeRetried_FailedOutsideDaysLimit_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.ExpiredFailedPayment();

        // Act
        var result = payment.CanBeRetried();

        // Assert
        result.Should().BeFalse();
        payment.ShouldNotBeRetryable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeRetried_CompletedPayment_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();

        // Act
        var result = payment.CanBeRetried();

        // Assert
        result.Should().BeFalse();
        payment.ShouldNotBeRetryable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeRefunded_CompletedPayment_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();

        // Act
        var result = payment.CanBeRefunded();

        // Assert
        result.Should().BeTrue();
        payment.ShouldBeRefundable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanBeRefunded_PendingPayment_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act
        var result = payment.CanBeRefunded();

        // Assert
        result.Should().BeFalse();
        payment.ShouldNotBeRefundable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanTransitionTo_ValidTransitions_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        payment.CanTransitionTo(PaymentStatus.Processing).Should().BeTrue();
        payment.CanTransitionTo(PaymentStatus.Completed).Should().BeTrue();
        payment.CanTransitionTo(PaymentStatus.Failed).Should().BeTrue();
        payment.CanTransitionTo(PaymentStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanTransitionTo_InvalidTransitions_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        payment.CanTransitionTo(PaymentStatus.Refunded).Should().BeFalse();
        payment.CanTransitionTo(PaymentStatus.PartiallyRefunded).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanTransitionTo_SameStatus_ShouldReturnTrue_Idempotent()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        payment.CanTransitionTo(PaymentStatus.Pending).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanTransitionTo_FromCancelled_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create()
            .WithStatus(PaymentStatus.Cancelled)
            .Build();

        // Act & Assert
        payment.CanTransitionTo(PaymentStatus.Processing).Should().BeFalse();
        payment.CanTransitionTo(PaymentStatus.Completed).Should().BeFalse();
        payment.CanTransitionTo(PaymentStatus.Failed).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsCompleted_WhenCompleted_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.CompletedPayment();

        // Act & Assert
        payment.IsCompleted.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsCompleted_WhenNotCompleted_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();

        // Act & Assert
        payment.IsCompleted.Should().BeFalse();
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();
        var domainEvent = new PaymentCompletedEvent(
            payment.Id,
            payment.SubscriptionId,
            payment.ClientId,
            payment.Amount,
            "ch_test_123",
            DateTime.UtcNow);

        // Act
        payment.AddDomainEvent(domainEvent);

        // Assert
        payment.DomainEvents.Should().ContainSingle();
        payment.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.Create().Build();
        payment.AddDomainEvent(new PaymentCompletedEvent(
            payment.Id,
            payment.SubscriptionId,
            payment.ClientId,
            payment.Amount,
            "ch_test_123",
            DateTime.UtcNow));

        // Act
        payment.ClearDomainEvents();

        // Assert
        payment.DomainEvents.Should().BeEmpty();
    }

    #endregion
}
