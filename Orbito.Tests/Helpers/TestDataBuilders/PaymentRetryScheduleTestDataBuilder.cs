using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

/// <summary>
/// Builder for creating PaymentRetrySchedule test data with fluent API
/// </summary>
public class PaymentRetryScheduleTestDataBuilder
{
    private TenantId _tenantId = TenantId.New();
    private Guid _id = Guid.NewGuid();
    private Guid _clientId = Guid.NewGuid();
    private Guid _paymentId = Guid.NewGuid();
    private int _attemptNumber = 1;
    private int _maxAttempts = 5;
    private RetryStatus _status = RetryStatus.Scheduled;
    private string? _lastError;
    private DateTime? _nextAttemptAt;
    private DateTime? _createdAt;
    private DateTime? _updatedAt;

    public static PaymentRetryScheduleTestDataBuilder Create() => new();

    public PaymentRetryScheduleTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithPaymentId(Guid paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithAttemptNumber(int attemptNumber)
    {
        _attemptNumber = attemptNumber;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithMaxAttempts(int maxAttempts)
    {
        _maxAttempts = maxAttempts;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithStatus(RetryStatus status)
    {
        _status = status;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithLastError(string lastError)
    {
        _lastError = lastError;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithNextAttemptAt(DateTime nextAttemptAt)
    {
        _nextAttemptAt = nextAttemptAt;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithScheduledAt(DateTime scheduledAt)
    {
        _nextAttemptAt = scheduledAt;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public PaymentRetryScheduleTestDataBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public PaymentRetrySchedule Build()
    {
        var schedule = PaymentRetrySchedule.Create(
            _tenantId,
            _clientId,
            _paymentId,
            _attemptNumber,
            _maxAttempts,
            _lastError);

        // Override properties that need custom values
        // Set Id if different from default
        var idProperty = typeof(PaymentRetrySchedule).GetProperty("Id");
        idProperty?.SetValue(schedule, _id);

        if (_nextAttemptAt.HasValue)
        {
            // Use reflection to set private property for testing
            var nextAttemptAtProperty = typeof(PaymentRetrySchedule).GetProperty("NextAttemptAt");
            nextAttemptAtProperty?.SetValue(schedule, _nextAttemptAt.Value);
        }

        if (_createdAt.HasValue)
        {
            var createdAtProperty = typeof(PaymentRetrySchedule).GetProperty("CreatedAt");
            createdAtProperty?.SetValue(schedule, _createdAt.Value);
        }

        if (_updatedAt.HasValue)
        {
            var updatedAtProperty = typeof(PaymentRetrySchedule).GetProperty("UpdatedAt");
            updatedAtProperty?.SetValue(schedule, _updatedAt.Value);
        }

        // Set status if different from default
        if (_status != RetryStatus.Scheduled)
        {
            var statusProperty = typeof(PaymentRetrySchedule).GetProperty("Status");
            statusProperty?.SetValue(schedule, _status);
        }

        return schedule;
    }

    // Predefined scenarios for common test cases
    public static PaymentRetrySchedule ScheduledRetry()
        => Create().Build();

    public static PaymentRetrySchedule InProgressRetry()
        => Create()
            .WithStatus(RetryStatus.InProgress)
            .Build();

    public static PaymentRetrySchedule CompletedRetry()
        => Create()
            .WithStatus(RetryStatus.Completed)
            .Build();

    public static PaymentRetrySchedule CancelledRetry()
        => Create()
            .WithStatus(RetryStatus.Cancelled)
            .Build();

    public static PaymentRetrySchedule FailedRetry()
        => Create()
            .WithStatus(RetryStatus.Failed)
            .WithLastError("Max attempts reached")
            .Build();

    public static PaymentRetrySchedule OverdueRetry()
        => Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(-10)) // 10 minutes overdue
            .Build();

    public static PaymentRetrySchedule DueRetry()
        => Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(-1)) // 1 minute ago (due)
            .Build();

    public static PaymentRetrySchedule FutureRetry()
        => Create()
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(5)) // 5 minutes in future
            .Build();

    public static PaymentRetrySchedule FirstAttemptRetry()
        => Create()
            .WithAttemptNumber(1)
            .Build();

    public static PaymentRetrySchedule SecondAttemptRetry()
        => Create()
            .WithAttemptNumber(2)
            .Build();

    public static PaymentRetrySchedule LastAttemptRetry()
        => Create()
            .WithAttemptNumber(5)
            .WithMaxAttempts(5)
            .Build();

    public static PaymentRetrySchedule RetryWithError(string error = "Network timeout")
        => Create()
            .WithLastError(error)
            .Build();

    public static PaymentRetrySchedule RetryWithCustomMaxAttempts(int maxAttempts)
        => Create()
            .WithMaxAttempts(maxAttempts)
            .Build();
}
