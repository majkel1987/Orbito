using FluentAssertions;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers.Assertions;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Domain.Entities;

public class PaymentRetryScheduleTests
{
    private readonly TenantId _tenantId = TenantId.New();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _paymentId = Guid.NewGuid();

    #region Creation & Validation Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithValidData_ShouldCreateSchedule()
    {
        // Arrange
        var attemptNumber = 1;
        var maxAttempts = 5;
        var lastError = "Network timeout";

        // Act
        var schedule = PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, attemptNumber, maxAttempts, lastError);

        // Assert
        schedule.Should().NotBeNull();
        schedule.TenantId.Should().Be(_tenantId);
        schedule.ClientId.Should().Be(_clientId);
        schedule.PaymentId.Should().Be(_paymentId);
        schedule.AttemptNumber.Should().Be(attemptNumber);
        schedule.MaxAttempts.Should().Be(maxAttempts);
        schedule.Status.Should().Be(RetryStatus.Scheduled);
        schedule.LastError.Should().Be(lastError);
        schedule.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        schedule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        schedule.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithInvalidAttemptNumber_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, 0, 5, null);
        action.Should().Throw<ArgumentException>()
            .WithMessage("AttemptNumber must be greater than 0*")
            .WithParameterName("attemptNumber");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithAttemptExceedingMax_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, 6, 5, null);
        action.Should().Throw<ArgumentException>()
            .WithMessage("AttemptNumber cannot exceed MaxAttempts*")
            .WithParameterName("attemptNumber");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidateAttemptNumber_WithValidRange_ShouldReturnTrue()
    {
        // Act & Assert
        PaymentRetrySchedule.ValidateAttemptNumber(1, 5).Should().BeTrue();
        PaymentRetrySchedule.ValidateAttemptNumber(3, 5).Should().BeTrue();
        PaymentRetrySchedule.ValidateAttemptNumber(5, 5).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidateAttemptNumber_WithInvalidRange_ShouldReturnFalse()
    {
        // Act & Assert
        PaymentRetrySchedule.ValidateAttemptNumber(0, 5).Should().BeFalse();
        PaymentRetrySchedule.ValidateAttemptNumber(-1, 5).Should().BeFalse();
        PaymentRetrySchedule.ValidateAttemptNumber(6, 5).Should().BeFalse();
    }

    #endregion

    #region Retry Logic Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void CalculateNextRetryTime_ShouldUseExponentialBackoff()
    {
        // Arrange
        var schedule1 = PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, 1, 5, null);
        var schedule2 = PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, 2, 5, null);
        var schedule3 = PaymentRetrySchedule.Create(_tenantId, _clientId, _paymentId, 3, 5, null);

        // Act
        var nextAttempt1 = schedule1.CalculateNextRetryTime();
        var nextAttempt2 = schedule2.CalculateNextRetryTime();
        var nextAttempt3 = schedule3.CalculateNextRetryTime();

        // Assert
        // 1st retry: 5 minutes
        nextAttempt1.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(10));
        
        // 2nd retry: 15 minutes
        nextAttempt2.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(10));
        
        // 3rd retry: 1 hour
        nextAttempt3.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(10));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanRetry_ScheduledAndTimeReached_ShouldReturnTrue()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(-1)) // 1 minute ago (due)
            .Build();

        // Act
        var result = schedule.CanRetry();

        // Assert
        result.Should().BeTrue();
        schedule.ShouldBeRetryable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanRetry_ScheduledButTimeNotReached_ShouldReturnFalse()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(5)) // 5 minutes in future
            .Build();

        // Act
        var result = schedule.CanRetry();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CanRetry_NotScheduled_ShouldReturnFalse()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

        // Act
        var result = schedule.CanRetry();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsInProgress_FromScheduled_ShouldUpdateStatus()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act
        schedule.MarkAsInProgress();

        // Assert
        schedule.Status.Should().Be(RetryStatus.InProgress);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsInProgress_FromNonScheduled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

        // Act & Assert
        var action = () => schedule.MarkAsInProgress();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only scheduled retries can be marked as in progress");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCompleted_FromInProgress_ShouldUpdateStatus()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

        // Act
        schedule.MarkAsCompleted();

        // Assert
        schedule.Status.Should().Be(RetryStatus.Completed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCompleted_FromNonInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act & Assert
        var action = () => schedule.MarkAsCompleted();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only in-progress retries can be marked as completed");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateForNextAttempt_ShouldCalculateNewTime()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .WithAttemptNumber(1)
            .Build();

        // Act
        schedule.UpdateForNextAttempt(2, "New error message");

        // Assert
        schedule.AttemptNumber.Should().Be(2);
        schedule.LastError.Should().Be("New error message");
        schedule.Status.Should().Be(RetryStatus.Scheduled);
        schedule.NextAttemptAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateForNextAttempt_FromNonInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act & Assert
        var action = () => schedule.UpdateForNextAttempt(2, "New error");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only in-progress retries can be updated");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsOverdue_PastScheduledTime_ShouldReturnTrue()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(-10)) // 10 minutes overdue
            .Build();

        // Act
        var result = schedule.IsOverdue();

        // Assert
        result.Should().BeTrue();
        schedule.ShouldBeOverdue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsOverdue_NotScheduled_ShouldReturnFalse()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

        // Act
        var result = schedule.IsOverdue();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasReachedMaxAttempts_AtMaxAttempts_ShouldReturnTrue()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithAttemptNumber(5)
            .WithMaxAttempts(5)
            .Build();

        // Act
        var result = schedule.HasReachedMaxAttempts();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasReachedMaxAttempts_BelowMaxAttempts_ShouldReturnFalse()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithAttemptNumber(3)
            .WithMaxAttempts(5)
            .Build();

        // Act
        var result = schedule.HasReachedMaxAttempts();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsFailed_FromInProgress_ShouldUpdateStatus()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

        // Act
        schedule.MarkAsFailed("Max attempts reached");

        // Assert
        schedule.Status.Should().Be(RetryStatus.Failed);
        schedule.LastError.Should().Be("Max attempts reached");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsFailed_FromNonInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act & Assert
        var action = () => schedule.MarkAsFailed("Error");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only in-progress retries can be marked as failed");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCancelled_ShouldUpdateStatus()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act
        schedule.MarkAsCancelled();

        // Assert
        schedule.Status.Should().Be(RetryStatus.Cancelled);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkAsCancelled_FromCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Completed)
            .Build();

        // Act & Assert
        var action = () => schedule.MarkAsCancelled();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Completed retries cannot be cancelled");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Cancel_ShouldCallMarkAsCancelled()
    {
        // Arrange
        var schedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithStatus(RetryStatus.Scheduled)
            .Build();

        // Act
        schedule.Cancel();

        // Assert
        schedule.Status.Should().Be(RetryStatus.Cancelled);
    }

    #endregion

    #region Predefined Scenarios Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void ScheduledRetry_ShouldBeScheduled()
    {
        // Act
        var schedule = PaymentRetryScheduleTestDataBuilder.ScheduledRetry();

        // Assert
        schedule.ShouldBeScheduledRetry();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void OverdueRetry_ShouldBeOverdue()
    {
        // Act
        var schedule = PaymentRetryScheduleTestDataBuilder.OverdueRetry();

        // Assert
        schedule.ShouldBeOverdue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DueRetry_ShouldBeRetryable()
    {
        // Act
        var schedule = PaymentRetryScheduleTestDataBuilder.DueRetry();

        // Assert
        schedule.ShouldBeRetryable();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void FutureRetry_ShouldNotBeRetryable()
    {
        // Act
        var schedule = PaymentRetryScheduleTestDataBuilder.FutureRetry();

        // Assert
        schedule.CanRetry().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void LastAttemptRetry_ShouldHaveReachedMaxAttempts()
    {
        // Act
        var schedule = PaymentRetryScheduleTestDataBuilder.LastAttemptRetry();

        // Assert
        schedule.HasReachedMaxAttempts().Should().BeTrue();
    }

    #endregion
}
