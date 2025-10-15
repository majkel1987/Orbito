using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Queries.GetPaymentMethodsByClient;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Queries.GetPaymentMethodsByClient;

[Trait("Category", "Unit")]
public class GetPaymentMethodsByClientQueryHandlerTests : BaseTestFixture
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<GetPaymentMethodsByClientQueryHandler>> _loggerMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly GetPaymentMethodsByClientQueryHandler _handler;

    public GetPaymentMethodsByClientQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<GetPaymentMethodsByClientQueryHandler>>();
        _tenantContextMock = new Mock<ITenantContext>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();

        // Setup UnitOfWork to return repositories
        _unitOfWorkMock.Setup(x => x.Clients).Returns(_clientRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.PaymentMethods).Returns(_paymentMethodRepositoryMock.Object);

        _handler = new GetPaymentMethodsByClientQueryHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _tenantContextMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidClient_ShouldReturnPaymentMethods()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = true
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var paymentMethods = new List<PaymentMethod>
        {
            PaymentMethodTestDataBuilder.Create()
                .WithClientId(TestClientId)
                .WithTenantId(TestTenantId)
                .WithType(PaymentMethodType.Card)
                .WithLastFourDigits("4242")
                .AsDefault()
                .Build(),
            PaymentMethodTestDataBuilder.Create()
                .WithClientId(TestClientId)
                .WithTenantId(TestTenantId)
                .WithType(PaymentMethodType.Card)
                .WithLastFourDigits("5555")
                .NotDefault()
                .Build()
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paymentMethods, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethods.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Success.Should().BeTrue();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()), Times.Once);
        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyPaymentMethods_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PaymentMethod>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethods.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 2,
            PageSize = 5
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var paymentMethods = new List<PaymentMethod>
        {
            PaymentMethodTestDataBuilder.Create()
                .WithClientId(TestClientId)
                .WithTenantId(TestTenantId)
                .Build()
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 2, 5, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paymentMethods, 7));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethods.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(7);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalPages.Should().Be(2); // Math.Ceiling(7/5) = 2
    }

    [Fact]
    public async Task Handle_WithTypeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10,
            Type = PaymentMethodType.Card
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var paymentMethods = new List<PaymentMethod>
        {
            PaymentMethodTestDataBuilder.Create()
                .WithClientId(TestClientId)
                .WithTenantId(TestTenantId)
                .WithType(PaymentMethodType.Card)
                .Build()
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, PaymentMethodType.Card, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paymentMethods, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethods.Should().HaveCount(1);

        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            TestClientId, 1, 10, PaymentMethodType.Card, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyFalse_ShouldReturnAllPaymentMethods()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10,
            ActiveOnly = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var paymentMethods = new List<PaymentMethod>
        {
            PaymentMethodTestDataBuilder.Create()
                .WithClientId(TestClientId)
                .WithTenantId(TestTenantId)
                .Build()
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paymentMethods, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            TestClientId, 1, 10, null, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PaymentMethodType?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithClientFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };
        var differentTenantId = TenantId.New();

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(differentTenantId) // Different tenant
            .Build();

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()), Times.Once);
        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PaymentMethodType?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentClient_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Client not found");
        result.Value.Should().BeNull();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WithClientRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error getting payment methods");
        result.Value.Should().BeNull();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPaymentMethodRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error getting payment methods");
        result.Value.Should().BeNull();

        _clientRepositoryMock.Verify(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()), Times.Once);
        _paymentMethodRepositoryMock.Verify(x => x.GetByClientIdWithCountAsync(
            TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Data Mapping Tests

    [Fact]
    public async Task Handle_ShouldMapPaymentMethodPropertiesCorrectly()
    {
        // Arrange
        var query = new GetPaymentMethodsByClientQuery
        {
            ClientId = TestClientId,
            PageNumber = 1,
            PageSize = 10
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithType(PaymentMethodType.Card)
            .WithLastFourDigits("1234")
            .WithExpiryDate(DateTime.UtcNow.AddYears(2))
            .AsDefault()
            .Build();

        var paymentMethods = new List<PaymentMethod> { paymentMethod };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdWithCountAsync(
                TestClientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((paymentMethods, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethods.Should().HaveCount(1);

        var paymentMethodDto = result.Value.PaymentMethods.First();
        paymentMethodDto.Id.Should().Be(paymentMethod.Id);
        paymentMethodDto.ClientId.Should().Be(TestClientId);
        paymentMethodDto.Type.Should().Be(PaymentMethodType.Card);
        paymentMethodDto.LastFourDigits.Should().Be("1234");
        paymentMethodDto.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddYears(2), TimeSpan.FromSeconds(1));
        paymentMethodDto.IsDefault.Should().BeTrue();
        paymentMethodDto.IsExpired.Should().BeFalse();
        paymentMethodDto.CanBeUsed.Should().BeTrue();
        paymentMethodDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion
}
