using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Commands.SavePaymentMethod;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands
{
    [Trait("Category", "Unit")]
    public class SavePaymentMethodCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<SavePaymentMethodCommandHandler>> _loggerMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly SavePaymentMethodCommandHandler _handler;

        public SavePaymentMethodCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<SavePaymentMethodCommandHandler>>();
            _tenantContextMock = new Mock<ITenantContext>();

            _handler = new SavePaymentMethodCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _tenantContextMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new SavePaymentMethodCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _tenantContextMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new SavePaymentMethodCommandHandler(
                    null!,
                    _loggerMock.Object,
                    _tenantContextMock.Object));

            exception.ParamName.Should().Be("unitOfWork");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new SavePaymentMethodCommandHandler(
                    _unitOfWorkMock.Object,
                    null!,
                    _tenantContextMock.Object));

            exception.ParamName.Should().Be("logger");
        }

        [Fact]
        public void Constructor_WithNullTenantContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new SavePaymentMethodCommandHandler(
                    _unitOfWorkMock.Object,
                    _loggerMock.Object,
                    null!));

            exception.ParamName.Should().Be("tenantContext");
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_WithValidRequest_ShouldSavePaymentMethod()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123",
                IsDefault = true
            };

            var client = CreateTestClient(clientId, tenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByClientIdAsync(clientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PaymentMethod>());

            _unitOfWorkMock.Setup(x => x.PaymentMethods.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentMethod());

            _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PaymentMethodId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Access denied");
        }

        [Fact]
        public async Task Handle_WithNonExistentClient_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Client not found");
        }

        [Fact]
        public async Task Handle_WithClientFromDifferentTenant_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var differentTenantId = TenantId.New();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            var client = CreateTestClient(clientId, differentTenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Access denied");
        }

        [Fact]
        public async Task Handle_WithTransactionFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            var client = CreateTestClient(clientId, tenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure("Transaction failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Transaction failed");
        }

        [Fact]
        public async Task Handle_WithCommitFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            var client = CreateTestClient(clientId, tenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByClientIdAsync(clientId, 1, 10, null, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PaymentMethod>());

            _unitOfWorkMock.Setup(x => x.PaymentMethods.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentMethod());

            _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure("Commit failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Commit failed");
        }

        [Fact]
        public async Task Handle_WithCancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new SavePaymentMethodCommand
            {
                ClientId = clientId,
                Type = PaymentMethodType.Card,
                Token = "encrypted_token_123"
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TenantId.New());

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }

        #endregion

        #region Helper Methods

        private Client CreateTestClient(Guid clientId, TenantId tenantId)
        {
            return Client.CreateDirect(
                tenantId,
                "client@example.com",
                "Test",
                "Client");
        }

        #endregion
    }
}
