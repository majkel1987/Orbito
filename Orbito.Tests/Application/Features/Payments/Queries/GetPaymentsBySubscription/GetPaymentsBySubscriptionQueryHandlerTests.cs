using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Features.Payments.Queries.GetPaymentsBySubscription;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Queries.GetPaymentsBySubscription;

[Trait("Category", "Unit")]
public class GetPaymentsBySubscriptionQueryHandlerTests : BaseTestFixture
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<GetPaymentsBySubscriptionQueryHandler>> _loggerMock;
    private readonly GetPaymentsBySubscriptionQueryHandler _handler;

    public GetPaymentsBySubscriptionQueryHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<GetPaymentsBySubscriptionQueryHandler>>();

        _handler = new GetPaymentsBySubscriptionQueryHandler(
            _paymentRepositoryMock.Object,
            _subscriptionRepositoryMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidSubscription_ShouldReturnPayments()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();

        var payments = new List<Payment>
        {
            PaymentTestDataBuilder.Create()
                .WithTenantId(TestTenantId)
                .WithClientId(TestClientId)
                .WithSubscriptionId(subscriptionId)
                .WithAmount(100.00m, "USD")
                .WithStatus(PaymentStatus.Completed)
                .Build(),
            PaymentTestDataBuilder.Create()
                .WithTenantId(TestTenantId)
                .WithClientId(TestClientId)
                .WithSubscriptionId(subscriptionId)
                .WithAmount(100.00m, "USD")
                .WithStatus(PaymentStatus.Pending)
                .Build()
        };

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _paymentRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);
        _paymentRepositoryMock.Setup(x => x.GetCountBySubscriptionIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.GetCountBySubscriptionIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyPayments_ShouldReturnEmptyList()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .Build();

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _paymentRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());
        _paymentRepositoryMock.Setup(x => x.GetCountBySubscriptionIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 2, 5);

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .Build();

        var payments = new List<Payment>
        {
            PaymentTestDataBuilder.Create()
                .WithTenantId(TestTenantId)
                .WithClientId(TestClientId)
                .WithSubscriptionId(subscriptionId)
                .Build()
        };

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _paymentRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subscriptionId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);
        _paymentRepositoryMock.Setup(x => x.GetCountBySubscriptionIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(7);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);

        _paymentRepositoryMock.Verify(x => x.GetBySubscriptionIdAsync(subscriptionId, 2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Tenant context is not available");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentRepositoryMock.Verify(x => x.GetBySubscriptionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSubscriptionFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);
        var differentTenantId = TenantId.New();

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(differentTenantId) // Different tenant
            .WithClientId(TestClientId)
            .Build();

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Subscription was not found");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.GetBySubscriptionIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentSubscription_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Subscription was not found");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 0, 10); // Invalid page number

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Page number must be greater than zero");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 0); // Invalid page size

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Page size must be between 1 and 100");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPageSizeTooLarge_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 101); // Page size too large

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Page size must be between 1 and 100");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WithSubscriptionRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("An unexpected error occurred");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPaymentRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .Build();

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _paymentRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("An unexpected error occurred");

        _subscriptionRepositoryMock.Verify(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task Handle_ShouldMapPaymentPropertiesCorrectly()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var query = new GetPaymentsBySubscriptionQuery(subscriptionId, TestClientId, 1, 10);

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(subscriptionId)
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .Build();

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(TestClientId)
            .WithSubscriptionId(subscriptionId)
            .WithAmount(150.75m, "EUR")
            .WithStatus(PaymentStatus.Completed)
            .WithExternalTransactionId("ch_test_789")
            .WithPaymentMethod(PaymentMethodType.Card.ToString())
            .WithProcessedAt(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        var payments = new List<Payment> { payment };

        _subscriptionRepositoryMock.Setup(x => x.GetByIdForClientAsync(subscriptionId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _paymentRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subscriptionId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);
        _paymentRepositoryMock.Setup(x => x.GetCountBySubscriptionIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().HaveCount(1);

        var paymentDto = result.Value.Payments.First();
        paymentDto.Id.Should().Be(payment.Id);
        paymentDto.TenantId.Should().Be(TestTenantId.Value);
        paymentDto.SubscriptionId.Should().Be(subscriptionId);
        paymentDto.ClientId.Should().Be(TestClientId);
        paymentDto.Amount.Should().Be(150.75m);
        paymentDto.Currency.Should().Be("EUR");
        paymentDto.Status.Should().Be(PaymentStatus.Completed.ToString());
        paymentDto.ExternalTransactionId.Should().Be("ch_test_789");
        paymentDto.PaymentMethod.Should().Be(PaymentMethodType.Card.ToString());
        paymentDto.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(-10), TimeSpan.FromSeconds(1));
    }

    #endregion
}
