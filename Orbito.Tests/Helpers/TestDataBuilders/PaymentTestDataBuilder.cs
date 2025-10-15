using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.TestDataBuilders;

/// <summary>
/// Builder for creating Payment test data with fluent API
/// </summary>
public class PaymentTestDataBuilder
{
    private TenantId _tenantId = TenantId.New();
    private Guid _id = Guid.NewGuid();
    private Guid _subscriptionId = Guid.NewGuid();
    private Guid _clientId = Guid.NewGuid();
    private Subscription? _subscription;
    private Money _amount = Money.Create(100.00m, "USD");
    private PaymentStatus _status = PaymentStatus.Pending;
    private string? _externalTransactionId;
    private string? _paymentMethod;
    private string? _externalPaymentId;
    private string? _paymentMethodId;
    private IdempotencyKey? _idempotencyKey;
    private string? _failureReason;
    private string? _refundReason;
    private DateTime? _processedAt;
    private DateTime? _failedAt;
    private DateTime? _refundedAt;

    public static PaymentTestDataBuilder Create() => new();

    public PaymentTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PaymentTestDataBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public PaymentTestDataBuilder WithSubscriptionId(Guid subscriptionId)
    {
        _subscriptionId = subscriptionId;
        return this;
    }

    public PaymentTestDataBuilder WithSubscription(Subscription subscription)
    {
        _subscription = subscription;
        return this;
    }

    public PaymentTestDataBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public PaymentTestDataBuilder WithAmount(decimal amount, string currency = "USD")
    {
        _amount = Money.Create(amount, currency);
        return this;
    }

    public PaymentTestDataBuilder WithStatus(PaymentStatus status)
    {
        _status = status;
        return this;
    }

    public PaymentTestDataBuilder WithExternalTransactionId(string externalTransactionId)
    {
        _externalTransactionId = externalTransactionId;
        return this;
    }

    public PaymentTestDataBuilder WithPaymentMethod(string paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public PaymentTestDataBuilder WithExternalPaymentId(string externalPaymentId)
    {
        _externalPaymentId = externalPaymentId;
        return this;
    }

    public PaymentTestDataBuilder WithPaymentMethodId(string paymentMethodId)
    {
        _paymentMethodId = paymentMethodId;
        return this;
    }

    public PaymentTestDataBuilder WithIdempotencyKey(IdempotencyKey idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public PaymentTestDataBuilder WithFailureReason(string failureReason)
    {
        _failureReason = failureReason;
        return this;
    }

    public PaymentTestDataBuilder WithRefundReason(string refundReason)
    {
        _refundReason = refundReason;
        return this;
    }

    public PaymentTestDataBuilder WithProcessedAt(DateTime processedAt)
    {
        _processedAt = processedAt;
        return this;
    }

    public PaymentTestDataBuilder WithFailedAt(DateTime failedAt)
    {
        _failedAt = failedAt;
        return this;
    }

    public PaymentTestDataBuilder WithRefundedAt(DateTime refundedAt)
    {
        _refundedAt = refundedAt;
        return this;
    }

    public Payment Build()
    {
        var payment = Payment.Create(
            _tenantId,
            _subscriptionId,
            _clientId,
            _amount,
            _externalTransactionId,
            _paymentMethod,
            _externalPaymentId,
            _idempotencyKey);

        // Set the specific ID if provided
        payment.Id = _id;

        // Set additional properties that aren't in the constructor
        payment.Status = _status;
        payment.PaymentMethodId = _paymentMethodId;
        payment.FailureReason = _failureReason;
        payment.RefundReason = _refundReason;
        payment.ProcessedAt = _processedAt;
        payment.FailedAt = _failedAt;
        payment.RefundedAt = _refundedAt;

        // Set navigation properties if provided
        if (_subscription != null)
        {
            payment.Subscription = _subscription;
        }

        return payment;
    }

    // Predefined scenarios for common test cases
    public static Payment ValidPayment() => Create().Build();

    public static Payment FailedPayment(string reason = "Insufficient funds")
        => Create()
            .WithStatus(PaymentStatus.Failed)
            .WithFailureReason(reason)
            .WithFailedAt(DateTime.UtcNow)
            .Build();

    public static Payment CompletedPayment()
        => Create()
            .WithStatus(PaymentStatus.Completed)
            .WithProcessedAt(DateTime.UtcNow)
            .WithExternalTransactionId("ch_test_123")
            .Build();

    public static Payment RefundedPayment(string reason = "Customer request")
        => Create()
            .WithStatus(PaymentStatus.Refunded)
            .WithProcessedAt(DateTime.UtcNow.AddDays(-1))
            .WithRefundedAt(DateTime.UtcNow)
            .WithRefundReason(reason)
            .WithExternalTransactionId("ch_test_123")
            .Build();

    public static Payment ProcessingPayment()
        => Create()
            .WithStatus(PaymentStatus.Processing)
            .Build();

    public static Payment CancelledPayment()
        => Create()
            .WithStatus(PaymentStatus.Cancelled)
            .WithFailedAt(DateTime.UtcNow)
            .WithFailureReason("Payment cancelled")
            .Build();

    public static Payment PartiallyRefundedPayment()
        => Create()
            .WithStatus(PaymentStatus.PartiallyRefunded)
            .WithProcessedAt(DateTime.UtcNow.AddDays(-1))
            .WithRefundedAt(DateTime.UtcNow)
            .WithRefundReason("Partial refund")
            .WithExternalTransactionId("ch_test_123")
            .Build();

    public static Payment PaymentWithIdempotencyKey(string key = "test-key-123")
        => Create()
            .WithIdempotencyKey(IdempotencyKey.Create(key))
            .Build();

    public static Payment ExpiredFailedPayment()
        => Create()
            .WithStatus(PaymentStatus.Failed)
            .WithFailedAt(DateTime.UtcNow.AddDays(-31)) // Outside 30-day retry window
            .WithFailureReason("Card declined")
            .Build();

    public static Payment RecentFailedPayment()
        => Create()
            .WithStatus(PaymentStatus.Failed)
            .WithFailedAt(DateTime.UtcNow.AddDays(-5)) // Within 30-day retry window
            .WithFailureReason("Network timeout")
            .Build();
}
