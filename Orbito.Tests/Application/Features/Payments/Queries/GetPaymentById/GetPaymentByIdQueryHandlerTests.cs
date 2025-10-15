using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Application.Features.Payments.Queries.GetPaymentById;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Queries.GetPaymentById;

[Trait("Category", "Unit")]
public class GetPaymentByIdQueryHandlerTests : BaseTestFixture
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetPaymentByIdQueryHandler>> _loggerMock;
    private readonly GetPaymentByIdQueryHandler _handler;

    public GetPaymentByIdQueryHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetPaymentByIdQueryHandler>>();

        _handler = new GetPaymentByIdQueryHandler(
            _paymentRepositoryMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidPayment_ShouldReturnSuccess()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        var payment = PaymentTestDataBuilder.Create()
            .WithId(paymentId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithAmount(100.00m, "USD")
            .WithStatus(PaymentStatus.Completed)
            .WithExternalTransactionId("ch_test_123")
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment.Should().NotBeNull();
        result.Payment!.Id.Should().Be(paymentId);
        result.Payment.TenantId.Should().Be(TestTenantId.Value);
        result.Payment.ClientId.Should().Be(TestClientId);
        result.Payment.Amount.Should().Be(100.00m);
        result.Payment.Currency.Should().Be("USD");
        result.Payment.Status.Should().Be(PaymentStatus.Completed.ToString());
        result.Payment.ExternalTransactionId.Should().Be("ch_test_123");

        _paymentRepositoryMock.Verify(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPendingPayment_ShouldReturnSuccess()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        var payment = PaymentTestDataBuilder.Create()
            .WithId(paymentId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithStatus(PaymentStatus.Pending)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment.Should().NotBeNull();
        result.Payment!.Status.Should().Be(PaymentStatus.Pending.ToString());
    }

    [Fact]
    public async Task Handle_WithFailedPayment_ShouldReturnSuccess()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        var payment = PaymentTestDataBuilder.Create()
            .WithId(paymentId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithStatus(PaymentStatus.Failed)
            .WithFailureReason("Card declined")
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment.Should().NotBeNull();
        result.Payment!.Status.Should().Be(PaymentStatus.Failed.ToString());
        result.Payment.FailureReason.Should().Be("Card declined");
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Tenant context is required");
        result.Payment.Should().BeNull();

        _paymentRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPaymentFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);
        var differentTenantId = TenantId.New();

        var payment = PaymentTestDataBuilder.Create()
            .WithId(paymentId)
            .WithTenantId(differentTenantId) // Different tenant
            .WithClientId(TestClientId)
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payment not found");
        result.Payment.Should().BeNull();

        _paymentRepositoryMock.Verify(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payment not found");
        result.Payment.Should().BeNull();

        _paymentRepositoryMock.Verify(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving payment");
        result.Payment.Should().BeNull();

        _paymentRepositoryMock.Verify(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTimeoutException_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Request timeout"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving payment");
        result.Payment.Should().BeNull();
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task Handle_ShouldMapAllPaymentProperties()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentByIdQuery(paymentId);

        var payment = PaymentTestDataBuilder.Create()
            .WithId(paymentId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithSubscriptionId(subscriptionId)
            .WithAmount(250.50m, "EUR")
            .WithStatus(PaymentStatus.Completed)
            .WithExternalTransactionId("ch_test_456")
            .WithPaymentMethod(PaymentMethodType.Card.ToString())
            .WithExternalPaymentId("pm_test_789")
            .WithPaymentMethodId("pm_123456")
            .WithProcessedAt(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Payment.Should().NotBeNull();

        var paymentDto = result.Payment!;
        paymentDto.Id.Should().Be(paymentId);
        paymentDto.TenantId.Should().Be(TestTenantId.Value);
        paymentDto.SubscriptionId.Should().Be(subscriptionId);
        paymentDto.ClientId.Should().Be(TestClientId);
        paymentDto.Amount.Should().Be(250.50m);
        paymentDto.Currency.Should().Be("EUR");
        paymentDto.Status.Should().Be(PaymentStatus.Completed.ToString());
        paymentDto.ExternalTransactionId.Should().Be("ch_test_456");
        paymentDto.PaymentMethod.Should().Be(PaymentMethodType.Card.ToString());
        paymentDto.ExternalPaymentId.Should().Be("pm_test_789");
        paymentDto.PaymentMethodId.Should().Be("pm_123456");
        paymentDto.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(-5), TimeSpan.FromSeconds(1));
    }

    #endregion
}
