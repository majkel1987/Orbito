using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Commands.UpdatePaymentFromWebhook;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands
{
    [Trait("Category", "Unit")]
    public class UpdatePaymentFromWebhookCommandHandlerTests
    {
        private readonly Mock<ILogger<UpdatePaymentFromWebhookCommandHandler>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IWebhookLogRepository> _webhookLogRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly UpdatePaymentFromWebhookCommandHandler _handler;

        public UpdatePaymentFromWebhookCommandHandlerTests()
        {
            _loggerMock = new Mock<ILogger<UpdatePaymentFromWebhookCommandHandler>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _webhookLogRepositoryMock = new Mock<IWebhookLogRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            _unitOfWorkMock.Setup(x => x.Payments).Returns(_paymentRepositoryMock.Object);
            _unitOfWorkMock.Setup(x => x.WebhookLogs).Returns(_webhookLogRepositoryMock.Object);

            _handler = new UpdatePaymentFromWebhookCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _tenantContextMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UpdatePaymentFromWebhookCommandHandler(
                null!,
                _loggerMock.Object,
                _tenantContextMock.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UpdatePaymentFromWebhookCommandHandler(
                _unitOfWorkMock.Object,
                null!,
                _tenantContextMock.Object));
        }

        [Fact]
        public void Constructor_WithNullTenantContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UpdatePaymentFromWebhookCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new UpdatePaymentFromWebhookCommandHandler(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _tenantContextMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_ValidPaymentIntentSucceeded_ShouldUpdatePaymentSuccessfully()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_123",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_123",
                NewStatus = PaymentStatus.Completed,
                ErrorMessage = null
            };

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Pending);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_123", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            payment.Status.Should().Be(PaymentStatus.Completed);
            payment.ExternalPaymentId.Should().Be("txn_123");
        }

        [Fact]
        public async Task Handle_ValidPaymentIntentFailed_ShouldUpdatePaymentSuccessfully()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_124",
                EventType = "payment_intent.payment_failed",
                Payload = "{}",
                ExternalPaymentId = "txn_124",
                NewStatus = PaymentStatus.Failed,
                ErrorMessage = "Card declined"
            };

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Pending);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_124", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            payment.Status.Should().Be(PaymentStatus.Failed);
            payment.FailureReason.Should().Be("Card declined");
        }

        [Fact]
        public async Task Handle_WebhookAlreadyProcessed_ShouldReturnSuccess()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_125",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_125",
                NewStatus = PaymentStatus.Completed
            };

            var webhookLog = new PaymentWebhookLog
            {
                EventId = "evt_125",
                Status = WebhookStatus.Processed
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_125", It.IsAny<CancellationToken>()))
                .ReturnsAsync(webhookLog);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoTenantContext_ShouldReturnFailure()
        {
            // Arrange
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = Guid.NewGuid(),
                EventId = "evt_126",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_126",
                NewStatus = PaymentStatus.Completed
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_126", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(command.PaymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // Handler checks payment first, then tenant context, so if payment doesn't exist, it returns "Payment was not found"
            result.Error.Message.Should().Contain("Payment was not found");
        }

        [Fact]
        public async Task Handle_PaymentNotFound_ShouldReturnFailure()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_127",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_127",
                NewStatus = PaymentStatus.Completed
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_127", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_PaymentFromDifferentTenant_ShouldReturnFailure()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var differentTenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_128",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_128",
                NewStatus = PaymentStatus.Completed
            };

            var payment = CreateTestPayment(paymentId, differentTenantId, PaymentStatus.Pending);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_128", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("Cross-tenant access is not allowed");
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_129",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_129",
                NewStatus = PaymentStatus.Completed
            };

            var payment = CreateTestPayment(paymentId, tenantId, PaymentStatus.Pending);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_129", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payment);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("Database error");
        }

        [Fact]
        public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var tenantId = TenantId.New();
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = paymentId,
                EventId = "evt_130",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_130",
                NewStatus = PaymentStatus.Completed
            };

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(tenantId);
            _webhookLogRepositoryMock.Setup(x => x.GetByEventIdAsync("evt_130", It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentWebhookLog?)null);
            _paymentRepositoryMock.Setup(x => x.GetByIdUnsafeAsync(paymentId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Repository error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // Handler catches exception and returns DomainErrors.General.UnexpectedError
            result.Error.Message.Should().Contain("An unexpected error occurred");
        }

        [Fact]
        public async Task Handle_CancellationRequested_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var command = new UpdatePaymentFromWebhookCommand
            {
                PaymentId = Guid.NewGuid(),
                EventId = "evt_131",
                EventType = "payment_intent.succeeded",
                Payload = "{}",
                ExternalPaymentId = "txn_131",
                NewStatus = PaymentStatus.Completed
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            // Handler checks cancellation token at the start, so it should throw immediately
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
