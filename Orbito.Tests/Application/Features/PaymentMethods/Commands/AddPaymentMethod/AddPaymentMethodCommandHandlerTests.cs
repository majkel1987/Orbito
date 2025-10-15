using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.PaymentMethods.Commands;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.PaymentMethods.Commands.AddPaymentMethod;

public class AddPaymentMethodCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ISecurityLimitService> _securityLimitServiceMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<AddPaymentMethodCommandHandler>> _loggerMock;
    private readonly AddPaymentMethodCommandHandler _handler;

    public AddPaymentMethodCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _securityLimitServiceMock = new Mock<ISecurityLimitService>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _loggerMock = new Mock<ILogger<AddPaymentMethodCommandHandler>>();

        // Setup UnitOfWork property access
        _unitOfWorkMock.Setup(x => x.PaymentMethods).Returns(_paymentMethodRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Clients).Returns(_clientRepositoryMock.Object);

        _handler = new AddPaymentMethodCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _tenantContextMock.Object,
            _securityLimitServiceMock.Object,
            _notificationServiceMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidCommand_ShouldAddPaymentMethodSuccessfully()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            SetAsDefault = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(5); // Under limit
        SetupCanAddPaymentMethod(true);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulPaymentMethodCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ClientId.Should().Be(TestClientId);
        result.Value.Type.Should().Be(PaymentMethodType.Card);
        result.Value.IsDefault.Should().BeFalse(); // Not set as default

        VerifyPaymentMethodAdded();
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithSetAsDefault_ShouldSetPaymentMethodAsDefault()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            SetAsDefault = true
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        var existingDefault = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(1);
        SetupCanAddPaymentMethod(true);
        SetupExistingDefaultPaymentMethod(existingDefault);
        SetupSuccessfulPaymentMethodCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsDefault.Should().BeTrue();

        // Verify old default was removed
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(existingDefault, It.IsAny<CancellationToken>()), Times.Once);
        VerifyPaymentMethodAdded();
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithFirstPaymentMethod_ShouldAutomaticallySetAsDefault()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            LastFourDigits = "4242",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            SetAsDefault = false // Not explicitly set, but should auto-default
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(0); // First payment method
        SetupCanAddPaymentMethod(true);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulPaymentMethodCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsDefault.Should().BeTrue(); // Auto-set as default

        VerifyPaymentMethodAdded();
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidExpiryDate_ShouldCalculateCorrectEndOfMonth()
    {
        // Arrange
        var expiryYear = DateTime.UtcNow.Year + 2;
        var expiryMonth = 2; // February
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            LastFourDigits = "4242",
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            SetAsDefault = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(1);
        SetupCanAddPaymentMethod(true);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulPaymentMethodCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        VerifyPaymentMethodAdded();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithoutExpiryDate_ShouldAddPaymentMethodSuccessfully()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.BankTransfer,
            Token = "tok_bank_123",
            LastFourDigits = "1234",
            SetAsDefault = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(1);
        SetupCanAddPaymentMethod(true);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulPaymentMethodCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        VerifyPaymentMethodAdded();
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            SetAsDefault = false
        };

        _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        VerifyPaymentMethodNotAdded();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonExistentClient_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            SetAsDefault = false
        };

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Client not found");
        result.Value.Should().BeNull();

        VerifyPaymentMethodNotAdded();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithClientFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            SetAsDefault = false
        };

        var differentTenantId = TenantId.New();
        var clientFromDifferentTenant = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(differentTenantId)
            .Build();

        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientFromDifferentTenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        VerifyPaymentMethodNotAdded();
    }

    #endregion

    #region Limit Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenLimitReached_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            SetAsDefault = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(10);
        SetupCanAddPaymentMethod(false); // Limit reached

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Maximum payment methods limit reached");

        VerifyPaymentMethodNotAdded();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithDatabaseException_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddPaymentMethodCommand
        {
            ClientId = TestClientId,
            Type = PaymentMethodType.Card,
            Token = "tok_test_123",
            SetAsDefault = false
        };

        var client = ClientTestDataBuilder.Create()
            .WithId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulClientLookup(client);
        SetupActiveCount(1);
        SetupCanAddPaymentMethod(true);

        _paymentMethodRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error adding payment method");
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulClientLookup(Client client)
    {
        _clientRepositoryMock.Setup(x => x.GetByIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
    }

    private void SetupActiveCount(int count)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetActiveCountByClientIdAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);
    }

    private void SetupCanAddPaymentMethod(bool canAdd)
    {
        _securityLimitServiceMock.Setup(x => x.CanAddPaymentMethod(TestClientId, It.IsAny<int>()))
            .Returns(canAdd);
        _securityLimitServiceMock.Setup(x => x.MaxPaymentMethodsPerClient)
            .Returns(10);
    }

    private void SetupNoDefaultPaymentMethods()
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod>());
    }

    private void SetupExistingDefaultPaymentMethod(PaymentMethod defaultPaymentMethod)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod> { defaultPaymentMethod });
    }

    private void SetupSuccessfulPaymentMethodCreation()
    {
        _paymentMethodRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethod pm, CancellationToken ct) => pm);
        _paymentMethodRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyPaymentMethodAdded()
    {
        _paymentMethodRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyPaymentMethodNotAdded()
    {
        _paymentMethodRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerifySaveChangesCalled()
    {
        _paymentMethodRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
