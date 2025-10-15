using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.PaymentMethods.Commands;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.PaymentMethods.Commands.SetDefaultPaymentMethod;

public class SetDefaultPaymentMethodCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<SetDefaultPaymentMethodCommandHandler>> _loggerMock;
    private readonly SetDefaultPaymentMethodCommandHandler _handler;

    public SetDefaultPaymentMethodCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<SetDefaultPaymentMethodCommandHandler>>();

        // Setup UnitOfWork property access
        _unitOfWorkMock.Setup(x => x.PaymentMethods).Returns(_paymentMethodRepositoryMock.Object);

        _handler = new SetDefaultPaymentMethodCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _tenantContextMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidCommand_ShouldSetPaymentMethodAsDefault()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithId(paymentMethodId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.AddYears(1))
            .Build();

        var currentDefault = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupCurrentDefaultPaymentMethod(currentDefault);
        SetupSuccessfulUpdate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethodId.Should().Be(paymentMethodId);
        result.Value.ClientId.Should().Be(TestClientId);
        result.Value.IsDefault.Should().BeTrue();
        result.Value.PreviousDefaultPaymentMethodId.Should().Be(currentDefault.Id);

        // Verify both payment methods were updated
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithAlreadyDefaultPaymentMethod_ShouldReturnSuccess()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithId(paymentMethodId)
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .WithExpiryDate(DateTime.UtcNow.AddYears(1))
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethodId.Should().Be(paymentMethodId);
        result.Value.IsDefault.Should().BeTrue();
        result.Value.PreviousDefaultPaymentMethodId.Should().BeNull();

        // Verify no updates were made
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentMethodRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNoCurrentDefault_ShouldSetAsDefault()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.AddYears(1))
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulUpdate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsDefault.Should().BeTrue();
        result.Value.PreviousDefaultPaymentMethodId.Should().BeNull();

        // Verify only one update (setting new default)
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifySaveChangesCalled();
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = Guid.NewGuid(),
            ClientId = TestClientId
        };

        _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        VerifyNoUpdates();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonExistentPaymentMethod_ShouldReturnFailure()
    {
        // Arrange
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = Guid.NewGuid(),
            ClientId = TestClientId
        };

        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(command.PaymentMethodId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethod?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Payment method not found");
        result.Value.Should().BeNull();

        VerifyNoUpdates();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPaymentMethodFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var differentTenantId = TenantId.New();
        var paymentMethodFromDifferentTenant = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(differentTenantId)
            .Build();

        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(paymentMethodId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethodFromDifferentTenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied");
        result.Value.Should().BeNull();

        VerifyNoUpdates();
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithExpiredPaymentMethod_ShouldReturnFailure()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var expiredPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.AddMonths(-1)) // Expired
            .Build();

        SetupSuccessfulPaymentMethodLookup(expiredPaymentMethod, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Payment method cannot be used");
        result.Value.Should().BeNull();

        VerifyNoUpdates();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPaymentMethodExpiringToday_ShouldSucceed()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.Date.AddDays(1)) // Still valid
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupNoDefaultPaymentMethods();
        SetupSuccessfulUpdate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithMultipleCurrentDefaults_ShouldRemoveAllDefaults()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.AddYears(1))
            .Build();

        var currentDefault1 = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        var currentDefault2 = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupMultipleDefaultPaymentMethods(currentDefault1, currentDefault2);
        SetupSuccessfulUpdate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify all defaults were removed and new one was set (3 updates total)
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        VerifySaveChangesCalled();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithDatabaseException_ShouldReturnFailure()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new SetDefaultPaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .WithExpiryDate(DateTime.UtcNow.AddYears(1))
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);

        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error setting default payment method");
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulPaymentMethodLookup(PaymentMethod paymentMethod, Guid paymentMethodId)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(paymentMethodId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethod);
    }

    private void SetupNoDefaultPaymentMethods()
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod>());
    }

    private void SetupCurrentDefaultPaymentMethod(PaymentMethod defaultPaymentMethod)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod> { defaultPaymentMethod });
    }

    private void SetupMultipleDefaultPaymentMethods(params PaymentMethod[] defaultPaymentMethods)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetDefaultPaymentMethodsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultPaymentMethods.ToList());
    }

    private void SetupSuccessfulUpdate()
    {
        _paymentMethodRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _paymentMethodRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifySaveChangesCalled()
    {
        _paymentMethodRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyNoUpdates()
    {
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentMethodRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
