using FluentAssertions;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.DTOs;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentStatus;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    [Trait("Category", "Unit")]
    public class UpdatePaymentStatusCommandHandlerTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly UpdatePaymentStatusCommandHandler _handler;

        public UpdatePaymentStatusCommandHandlerTests()
        {
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new UpdatePaymentStatusCommandHandler(
                _paymentRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new UpdatePaymentStatusCommandHandler(
                _paymentRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullPaymentRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new UpdatePaymentStatusCommandHandler(
                    null!,
                    _tenantContextMock.Object,
                    _unitOfWorkMock.Object));

            exception.ParamName.Should().Be("paymentRepository");
        }

        [Fact]
        public void Constructor_WithNullTenantContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new UpdatePaymentStatusCommandHandler(
                    _paymentRepositoryMock.Object,
                    null!,
                    _unitOfWorkMock.Object));

            exception.ParamName.Should().Be("tenantContext");
        }

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new UpdatePaymentStatusCommandHandler(
                    _paymentRepositoryMock.Object,
                    _tenantContextMock.Object,
                    null!));

            exception.ParamName.Should().Be("unitOfWork");
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_WithValidRequest_ShouldUpdatePaymentStatus()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Processing);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            _paymentRepositoryMock.Setup(x => x.UpdateAsync(payment, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Payment.Should().NotBeNull();
            result.Payment!.Status.Should().Be(PaymentStatus.Completed.ToString());
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Tenant context is required");
        }

        [Fact]
        public async Task Handle_WithNonExistentPayment_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Payment with ID");
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_WithPaymentFromDifferentTenant_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var differentTenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            var payment = CreateTestPayment(paymentId, differentTenantId, PaymentStatus.Processing);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Payment does not belong to current tenant");
        }

        [Fact]
        public async Task Handle_WithInvalidStatusTransition_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Completed);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid status transition");
        }

        [Fact]
        public async Task Handle_WithFailedStatusWithoutReason_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Failed);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Processing);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("FailureReason is required when marking payment as Failed");
        }

        [Fact]
        public async Task Handle_WithRefundedStatusWithoutReason_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Refunded);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Completed);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("RefundReason is required when marking payment as Refunded");
        }

        [Fact]
        public async Task Handle_WithPartiallyRefundedStatusWithoutReason_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.PartiallyRefunded);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Completed);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("RefundReason is required when marking payment as PartiallyRefunded");
        }

        [Fact]
        public async Task Handle_WithUnsupportedStatus_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                (PaymentStatus)999); // Unsupported status

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Pending);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Unsupported payment status");
        }

        [Fact]
        public async Task Handle_WithSaveChangesFailure_ShouldReturnError()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var clientId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Processing);
            payment.ClientId = clientId;

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);

            _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            _paymentRepositoryMock.Setup(x => x.UpdateAsync(payment, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Save failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Save failed");
        }

        [Fact]
        public async Task Handle_WithCancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentStatusCommand(
                paymentId,
                PaymentStatus.Completed);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(TenantId.New());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }

        #endregion

        #region Helper Methods

        private Payment CreateTestPayment(Guid paymentId, TenantId tenantId, PaymentStatus status)
        {
            return Payment.Create(
                tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Money.Create(100, "USD"));
        }

        #endregion
    }
}
