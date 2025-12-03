using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands.RefundPayment;

[Trait("Category", "Unit")]
public class RefundPaymentCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<IPaymentProcessingService> _paymentProcessingServiceMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ILogger<RefundPaymentCommandHandler>> _loggerMock;
    private readonly RefundPaymentCommandHandler _handler;

    public RefundPaymentCommandHandlerTests()
    {
        _paymentProcessingServiceMock = new Mock<IPaymentProcessingService>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _loggerMock = new Mock<ILogger<RefundPaymentCommandHandler>>();

        // Setup UnitOfWork to return payment repository
        UnitOfWorkMock.Setup(x => x.Payments).Returns(_paymentRepositoryMock.Object);

        _handler = new RefundPaymentCommandHandler(
            _paymentProcessingServiceMock.Object,
            UnitOfWorkMock.Object,
            TenantContextMock.Object,
            _loggerMock.Object);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidFullRefund_ShouldRefundSuccessfully()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Customer request"
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(TestSubscriptionId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.Client = client;

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithAmount(100.00m, "USD")
            .WithStatus(PaymentStatus.Completed)
            .WithSubscription(subscription)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var refundResult = RefundResult.Success(
            RefundStatus.Completed,
            "re_test_123");

        _paymentProcessingServiceMock.Setup(x => x.RefundPaymentAsync(
                paymentId,
                It.IsAny<Money>(),
                command.Reason,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalRefundId.Should().Be("re_test_123");
        result.Value.Status.Should().Be(RefundStatus.Completed.ToString());
        result.Error.Should().Be(Error.None);

        _paymentProcessingServiceMock.Verify(x => x.RefundPaymentAsync(
            paymentId,
            It.Is<Money>(m => m.Amount == 100.00m && m.Currency == "USD"),
            command.Reason,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidPartialRefund_ShouldRefundSuccessfully()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 50.00m,
            Currency = "USD",
            Reason = "Partial refund"
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(TestSubscriptionId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.Client = client;

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithAmount(100.00m, "USD")
            .WithStatus(PaymentStatus.Completed)
            .WithSubscription(subscription)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var refundResult = RefundResult.Success(
            RefundStatus.PartiallyRefunded,
            "re_test_456");

        _paymentProcessingServiceMock.Setup(x => x.RefundPaymentAsync(
                paymentId,
                It.IsAny<Money>(),
                command.Reason,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalRefundId.Should().Be("re_test_456");
        result.Value.Status.Should().Be(RefundStatus.PartiallyRefunded.ToString());
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        TenantContextMock.Setup(x => x.HasTenant).Returns(false);

        var command = new RefundPaymentCommand
        {
            PaymentId = Guid.NewGuid(),
            ClientId = TestClientId,
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Test"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Tenant context is not available");
        result.Error.Code.Should().Be("Tenant.NoTenantContext");
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Test"
        };

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Payment was not found");
        result.Error.Code.Should().Be("Payment.NotFound");
    }

    [Fact]
    public async Task Handle_WithRefundAmountExceedingPaymentAmount_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 150.00m,
            Currency = "USD",
            Reason = "Test"
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(TestSubscriptionId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.Client = client;

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithAmount(100.00m, "USD")
            .WithStatus(PaymentStatus.Completed)
            .WithSubscription(subscription)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        // Handler uses AmountMismatch error for refund amount exceeding payment amount
        result.Error.Message.Should().Contain("Payment amount does not match subscription amount");
        result.Error.Code.Should().Be("Payment.AmountMismatch");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenPaymentProcessingServiceFails_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Test"
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(TestSubscriptionId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.Client = client;

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithAmount(100.00m, "USD")
            .WithStatus(PaymentStatus.Completed)
            .WithSubscription(subscription)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var refundResult = RefundResult.Failure(
            "Payment gateway error",
            "GATEWAY_ERROR");

        _paymentProcessingServiceMock.Setup(x => x.RefundPaymentAsync(
                paymentId,
                It.IsAny<Money>(),
                command.Reason,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refundResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        // Handler returns ProcessingFailed error when payment gateway fails
        result.Error.Message.Should().Be("Payment processing failed");
        result.Error.Code.Should().Be("Payment.ProcessingFailed");
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Amount = 100.00m,
            Currency = "USD",
            Reason = "Test"
        };

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, TestClientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        // Handler catches exception and returns DomainErrors.General.UnexpectedError
        result.Error.Message.Should().Be("An unexpected error occurred");
        result.Error.Code.Should().Be("General.UnexpectedError");
    }

    #endregion
}

