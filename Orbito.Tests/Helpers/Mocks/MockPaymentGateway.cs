using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers.Mocks;

/// <summary>
/// Mock implementation of IPaymentGateway for testing
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly Dictionary<string, PaymentResult> _configuredResults = new();
    private readonly List<ProcessPaymentRequest> _processedRequests = new();
    private readonly List<RefundRequest> _processedRefunds = new();
    private readonly List<CreateCustomerRequest> _processedCustomers = new();

    // Configuration flags
    public bool ShouldThrowException { get; set; } = false;
    public string ExceptionMessage { get; set; } = "Mock gateway exception";
    public bool ShouldTimeout { get; set; } = false;
    public TimeSpan TimeoutDuration { get; set; } = TimeSpan.FromSeconds(5);
    public bool ShouldReturnNetworkError { get; set; } = false;

    // Default responses
    public PaymentResult DefaultPaymentResult { get; set; } = PaymentResult.Success(PaymentStatus.Completed, "ch_test_123");
    public RefundResult DefaultRefundResult { get; set; } = RefundResult.Success(RefundStatus.Completed, "re_test_123");
    public CustomerResult DefaultCustomerResult { get; set; } = CustomerResult.Success("cus_test_123", "test@example.com");

    public Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        _processedRequests.Add(request);

        if (ShouldThrowException)
            throw new InvalidOperationException(ExceptionMessage);

        if (ShouldTimeout)
            Thread.Sleep(TimeoutDuration);

        if (ShouldReturnNetworkError)
            return Task.FromResult(PaymentResult.Failure("Network connection failed"));

        // Check for configured result
        var key = $"payment_{request.Amount.Amount}_{request.Amount.Currency}";
        if (_configuredResults.TryGetValue(key, out var configuredResult))
            return Task.FromResult(configuredResult);

        // Check for specific failure scenarios
        if (request.Amount.Amount < 0.50m)
            return Task.FromResult(PaymentResult.Failure("Amount too small", "AMOUNT_TOO_SMALL"));

        if (request.Amount.Amount > 999999.99m)
            return Task.FromResult(PaymentResult.Failure("Amount too large", "AMOUNT_TOO_LARGE"));

        if (request.PaymentMethodId?.Contains("declined") == true)
            return Task.FromResult(PaymentResult.Failure("Your card was declined", "CARD_DECLINED"));

        if (request.PaymentMethodId?.Contains("insufficient") == true)
            return Task.FromResult(PaymentResult.Failure("Insufficient funds", "INSUFFICIENT_FUNDS"));

        if (request.PaymentMethodId?.Contains("expired") == true)
            return Task.FromResult(PaymentResult.Failure("Your card has expired", "CARD_EXPIRED"));

        return Task.FromResult(DefaultPaymentResult);
    }

    public Task<RefundResult> RefundPaymentAsync(RefundRequest request)
    {
        _processedRefunds.Add(request);

        if (ShouldThrowException)
            throw new InvalidOperationException(ExceptionMessage);

        if (ShouldTimeout)
            Thread.Sleep(TimeoutDuration);

        if (ShouldReturnNetworkError)
            return Task.FromResult(RefundResult.Failure("Network connection failed"));

        // Check for specific failure scenarios
        if (request.Amount.Amount < 0.01m)
            return Task.FromResult(RefundResult.Failure("Refund amount too small", "REFUND_AMOUNT_TOO_SMALL"));

        if (request.ExternalPaymentId?.Contains("already_refunded") == true)
            return Task.FromResult(RefundResult.Failure("Payment already refunded", "ALREADY_REFUNDED"));

        return Task.FromResult(DefaultRefundResult);
    }

    public Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request)
    {
        _processedCustomers.Add(request);

        if (ShouldThrowException)
            throw new InvalidOperationException(ExceptionMessage);

        if (ShouldTimeout)
            Thread.Sleep(TimeoutDuration);

        if (ShouldReturnNetworkError)
            return Task.FromResult(CustomerResult.Failure("Network connection failed"));

        // Check for specific failure scenarios
        if (string.IsNullOrWhiteSpace(request.Email))
            return Task.FromResult(CustomerResult.Failure("Email is required", "EMAIL_REQUIRED"));

        if (request.Email?.Contains("invalid") == true)
            return Task.FromResult(CustomerResult.Failure("Invalid email format", "INVALID_EMAIL"));

        return Task.FromResult(DefaultCustomerResult);
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string externalPaymentId)
    {
        if (ShouldThrowException)
            throw new InvalidOperationException(ExceptionMessage);

        if (ShouldTimeout)
            Thread.Sleep(TimeoutDuration);

        if (ShouldReturnNetworkError)
            return Task.FromResult(PaymentStatusResult.Failure("Network connection failed"));

        // Simulate different statuses based on payment ID
        var status = externalPaymentId switch
        {
            var id when id.Contains("succeeded") => PaymentStatus.Completed,
            var id when id.Contains("pending") => PaymentStatus.Pending,
            var id when id.Contains("failed") => PaymentStatus.Failed,
            var id when id.Contains("canceled") => PaymentStatus.Cancelled,
            _ => PaymentStatus.Completed
        };

        return Task.FromResult(PaymentStatusResult.Success(status, "ch_test_123"));
    }

    public Task<WebhookValidationResult> ValidateWebhookAsync(string payload, string signature)
    {
        if (ShouldThrowException)
            throw new InvalidOperationException(ExceptionMessage);

        if (ShouldTimeout)
            Thread.Sleep(TimeoutDuration);

        if (ShouldReturnNetworkError)
            return Task.FromResult(WebhookValidationResult.Failure("Network connection failed"));

        // Simulate validation based on signature
        if (signature?.Contains("invalid") == true)
            return Task.FromResult(WebhookValidationResult.Failure("Invalid signature"));

        if (string.IsNullOrWhiteSpace(signature))
            return Task.FromResult(WebhookValidationResult.Failure("Missing signature"));

        return Task.FromResult(WebhookValidationResult.Success(new { }, "test_event", DateTime.UtcNow, new Dictionary<string, string>()));
    }

    // Test helper methods
    public void ConfigurePaymentResult(string key, PaymentResult result)
    {
        _configuredResults[key] = result;
    }

    public void Reset()
    {
        _configuredResults.Clear();
        _processedRequests.Clear();
        _processedRefunds.Clear();
        _processedCustomers.Clear();
        ShouldThrowException = false;
        ShouldTimeout = false;
        ShouldReturnNetworkError = false;
    }

    public IReadOnlyList<ProcessPaymentRequest> ProcessedRequests => _processedRequests.AsReadOnly();
    public IReadOnlyList<RefundRequest> ProcessedRefunds => _processedRefunds.AsReadOnly();
    public IReadOnlyList<CreateCustomerRequest> ProcessedCustomers => _processedCustomers.AsReadOnly();
}