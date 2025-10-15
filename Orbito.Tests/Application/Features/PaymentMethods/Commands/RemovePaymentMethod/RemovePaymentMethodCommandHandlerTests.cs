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

namespace Orbito.Tests.Application.Features.PaymentMethods.Commands.RemovePaymentMethod;

public class RemovePaymentMethodCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<RemovePaymentMethodCommandHandler>> _loggerMock;
    private readonly RemovePaymentMethodCommandHandler _handler;

    public RemovePaymentMethodCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _loggerMock = new Mock<ILogger<RemovePaymentMethodCommandHandler>>();

        // Setup UnitOfWork property access
        _unitOfWorkMock.Setup(x => x.PaymentMethods).Returns(_paymentMethodRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Subscriptions).Returns(_subscriptionRepositoryMock.Object);

        _handler = new RemovePaymentMethodCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _tenantContextMock.Object,
            _notificationServiceMock.Object);

        // Setup default tenant context
        _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
        _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    #region Success Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidCommand_ShouldRemovePaymentMethodSuccessfully()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .Build();

        var otherPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupNoActiveSubscriptions();
        SetupMultiplePaymentMethods(paymentMethod, otherPaymentMethod);
        SetupSuccessfulDeletion();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PaymentMethodId.Should().Be(paymentMethodId);
        result.Value.ClientId.Should().Be(TestClientId);
        result.Value.WasDefault.Should().BeFalse();

        VerifyPaymentMethodDeleted();
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithDefaultPaymentMethod_ShouldSetAnotherAsDefault()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var defaultPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        var otherPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .Build();

        SetupSuccessfulPaymentMethodLookup(defaultPaymentMethod, paymentMethodId);
        SetupNoActiveSubscriptions();
        SetupMultiplePaymentMethods(defaultPaymentMethod, otherPaymentMethod);
        SetupSuccessfulDeletion();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.WasDefault.Should().BeTrue();
        result.Value.NewDefaultPaymentMethodId.Should().NotBeNull();

        // Verify another payment method was set as default
        _paymentMethodRepositoryMock.Verify(x => x.UpdateAsync(It.Is<PaymentMethod>(pm => pm.IsDefault), It.IsAny<CancellationToken>()), Times.Once);
        VerifyPaymentMethodDeleted();
        VerifySaveChangesCalled();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithLastPaymentMethodNoSubscriptions_ShouldRemoveSuccessfully()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupNoActiveSubscriptions();
        SetupSinglePaymentMethod(paymentMethod);
        SetupSuccessfulDeletion();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.WasDefault.Should().BeTrue();
        result.Value.NewDefaultPaymentMethodId.Should().BeNull(); // No other payment methods

        VerifyPaymentMethodDeleted();
        VerifySaveChangesCalled();
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemovePaymentMethodCommand
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

        VerifyPaymentMethodNotDeleted();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonExistentPaymentMethod_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemovePaymentMethodCommand
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

        VerifyPaymentMethodNotDeleted();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPaymentMethodFromDifferentTenant_ShouldReturnFailure()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
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

        VerifyPaymentMethodNotDeleted();
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithLastPaymentMethodAndActiveSubscriptions_ShouldReturnFailure()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        var activeSubscription = SubscriptionTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupActiveSubscriptions(activeSubscription);
        SetupSinglePaymentMethod(paymentMethod);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot remove the last payment method while you have active subscriptions");
        result.Value.Should().BeNull();

        VerifyPaymentMethodNotDeleted();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithMultiplePaymentMethodsAndActiveSubscriptions_ShouldSucceed()
    {
        // Arrange
        var paymentMethodId = Guid.NewGuid();
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .NotDefault()
            .Build();

        var otherPaymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .AsDefault()
            .Build();

        var activeSubscription = SubscriptionTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .WithStatus(SubscriptionStatus.Active)
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);
        SetupActiveSubscriptions(activeSubscription);
        SetupMultiplePaymentMethods(paymentMethod, otherPaymentMethod);
        SetupSuccessfulDeletion();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        VerifyPaymentMethodDeleted();
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
        var command = new RemovePaymentMethodCommand
        {
            PaymentMethodId = paymentMethodId,
            ClientId = TestClientId
        };

        var paymentMethod = PaymentMethodTestDataBuilder.Create()
            .WithClientId(TestClientId)
            .WithTenantId(TestTenantId)
            .Build();

        SetupSuccessfulPaymentMethodLookup(paymentMethod, paymentMethodId);

        _subscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error removing payment method");
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulPaymentMethodLookup(PaymentMethod paymentMethod, Guid paymentMethodId)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetByIdAsync(paymentMethodId, TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethod);
    }

    private void SetupNoActiveSubscriptions()
    {
        _subscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription>());
    }

    private void SetupActiveSubscriptions(params Subscription[] subscriptions)
    {
        _subscriptionRepositoryMock.Setup(x => x.GetActiveSubscriptionsByClientAsync(TestClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions.ToList());
    }

    private void SetupSinglePaymentMethod(PaymentMethod paymentMethod)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdAsync(
            TestClientId, 1, 100, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod> { paymentMethod });
    }

    private void SetupMultiplePaymentMethods(params PaymentMethod[] paymentMethods)
    {
        _paymentMethodRepositoryMock.Setup(x => x.GetByClientIdAsync(
            TestClientId, 1, 100, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods.ToList());
    }

    private void SetupSuccessfulDeletion()
    {
        _paymentMethodRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _paymentMethodRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyPaymentMethodDeleted()
    {
        _paymentMethodRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyPaymentMethodNotDeleted()
    {
        _paymentMethodRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerifySaveChangesCalled()
    {
        _paymentMethodRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
