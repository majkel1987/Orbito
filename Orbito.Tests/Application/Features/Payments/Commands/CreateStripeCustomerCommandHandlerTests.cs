using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands
{
    [Trait("Category", "Unit")]
    public class CreateStripeCustomerCommandHandlerTests
    {
        private readonly Mock<IPaymentProcessingService> _paymentProcessingServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<ILogger<CreateStripeCustomerCommandHandler>> _loggerMock;
        private readonly CreateStripeCustomerCommandHandler _handler;

        public CreateStripeCustomerCommandHandlerTests()
        {
            _paymentProcessingServiceMock = new Mock<IPaymentProcessingService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tenantContextMock = new Mock<ITenantContext>();
            _loggerMock = new Mock<ILogger<CreateStripeCustomerCommandHandler>>();

            _handler = new CreateStripeCustomerCommandHandler(
                _paymentProcessingServiceMock.Object,
                _unitOfWorkMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new CreateStripeCustomerCommandHandler(
                _paymentProcessingServiceMock.Object,
                _unitOfWorkMock.Object,
                _tenantContextMock.Object,
                _loggerMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullPaymentProcessingService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CreateStripeCustomerCommandHandler(
                    null!,
                    _unitOfWorkMock.Object,
                    _tenantContextMock.Object,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("paymentProcessingService");
        }

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CreateStripeCustomerCommandHandler(
                    _paymentProcessingServiceMock.Object,
                    null!,
                    _tenantContextMock.Object,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("unitOfWork");
        }

        [Fact]
        public void Constructor_WithNullTenantContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CreateStripeCustomerCommandHandler(
                    _paymentProcessingServiceMock.Object,
                    _unitOfWorkMock.Object,
                    null!,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("tenantContext");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CreateStripeCustomerCommandHandler(
                    _paymentProcessingServiceMock.Object,
                    _unitOfWorkMock.Object,
                    _tenantContextMock.Object,
                    null!));

            exception.ParamName.Should().Be("logger");
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_WithValidRequest_ShouldCreateStripeCustomer()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                CompanyName = "Test Company",
                Phone = "+1234567890"
            };

            var client = CreateTestClient(clientId, tenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            _paymentProcessingServiceMock.Setup(x => x.CreateCustomerAsync(
                    clientId,
                    command.Email,
                    command.FirstName,
                    command.LastName,
                    command.CompanyName,
                    command.Phone,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerResult
                {
                    IsSuccess = true,
                    ExternalCustomerId = "cus_stripe123",
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.StripeCustomerId.Should().Be("cus_stripe123");
            result.Value.Email.Should().Be(command.Email);
            result.Value.FirstName.Should().Be(command.FirstName);
            result.Value.LastName.Should().Be(command.LastName);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("Tenant context is not available");
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
        }

        [Fact]
        public async Task Handle_WithNonExistentClient_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
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
            result.Error.Message.Should().Contain("Client was not found");
            result.Error.Code.Should().Be("Client.NotFound");
        }

        [Fact]
        public async Task Handle_WithClientFromDifferentTenant_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var differentTenantId = TenantId.New();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
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
            result.Error.Message.Should().Contain("Cross-tenant access is not allowed");
            result.Error.Code.Should().Be("Tenant.CrossTenantAccess");
        }

        [Fact]
        public async Task Handle_WithPaymentProcessingServiceFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var client = CreateTestClient(clientId, tenantId);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            _paymentProcessingServiceMock.Setup(x => x.CreateCustomerAsync(
                    clientId,
                    command.Email,
                    command.FirstName,
                    command.LastName,
                    command.CompanyName,
                    command.Phone,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Stripe API error",
                    ErrorCode = "STRIPE_API_ERROR"
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("An unexpected error occurred");
            result.Error.Code.Should().Be("General.UnexpectedError");
        }

        [Fact]
        public async Task Handle_WithException_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TenantId.New());

            _unitOfWorkMock.Setup(x => x.Clients.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("An unexpected error occurred");
            result.Error.Code.Should().Be("General.UnexpectedError");
        }

        [Fact]
        public async Task Handle_WithCancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new CreateStripeCustomerCommand
            {
                ClientId = clientId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
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
