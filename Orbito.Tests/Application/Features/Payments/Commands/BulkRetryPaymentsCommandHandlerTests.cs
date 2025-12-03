using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands
{
    [Trait("Category", "Unit")]
    public class BulkRetryPaymentsCommandHandlerTests
    {
        private readonly Mock<IPaymentRetryService> _retryServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILogger<BulkRetryPaymentsCommandHandler>> _loggerMock;
        private readonly BulkRetryPaymentsCommandHandler _handler;

        public BulkRetryPaymentsCommandHandlerTests()
        {
            _retryServiceMock = new Mock<IPaymentRetryService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggerMock = new Mock<ILogger<BulkRetryPaymentsCommandHandler>>();

            _handler = new BulkRetryPaymentsCommandHandler(
                _retryServiceMock.Object,
                _unitOfWorkMock.Object,
                _userContextServiceMock.Object,
                _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new BulkRetryPaymentsCommandHandler(
                _retryServiceMock.Object,
                _unitOfWorkMock.Object,
                _userContextServiceMock.Object,
                _loggerMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullRetryService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BulkRetryPaymentsCommandHandler(
                    null!,
                    _unitOfWorkMock.Object,
                    _userContextServiceMock.Object,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("retryService");
        }

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BulkRetryPaymentsCommandHandler(
                    _retryServiceMock.Object,
                    null!,
                    _userContextServiceMock.Object,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("unitOfWork");
        }

        [Fact]
        public void Constructor_WithNullUserContextService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BulkRetryPaymentsCommandHandler(
                    _retryServiceMock.Object,
                    _unitOfWorkMock.Object,
                    null!,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("userContextService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new BulkRetryPaymentsCommandHandler(
                    _retryServiceMock.Object,
                    _unitOfWorkMock.Object,
                    _userContextServiceMock.Object,
                    null!));

            exception.ParamName.Should().Be("logger");
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_WithValidRequest_ShouldProcessBulkRetry()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            var payments = new Dictionary<Guid, Payment>
            {
                { paymentIds[0], CreateTestPayment(paymentIds[0], clientId) },
                { paymentIds[1], CreateTestPayment(paymentIds[1], clientId) }
            };

            var activeRetries = new Dictionary<Guid, PaymentRetrySchedule>();

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSpan?)null);

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdsForClientAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _retryServiceMock.Setup(x => x.GetActiveRetriesForPaymentsAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRetries);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _retryServiceMock.Setup(x => x.ScheduleRetryAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentRetrySchedule.Create(TenantId.New(), Guid.NewGuid(), Guid.NewGuid(), 1));

            _unitOfWorkMock.Setup(x => x.Payments.RecordPaymentAttemptsAsync(clientId, 2, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Value.TotalProcessed.Should().Be(2);
            result.Value.SuccessfulRetries.Should().Be(2);
            result.Value.FailedRetries.Should().Be(0);
            result.Value.Results.Should().HaveCount(2);
            result.Value.Results.All(r => r.Success).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithTooManyPayments_ShouldReturnRateLimitError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = Enumerable.Range(0, 51).Select(_ => Guid.NewGuid()).ToList();
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Contain("PaymentIds must be between 1 and 50");
        }

        [Fact]
        public async Task Handle_WithDifferentClientId_ShouldReturnSecurityError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var differentClientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(differentClientId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Be("Unauthorized access");
        }

        [Fact]
        public async Task Handle_WithRateLimitExceeded_ShouldReturnRateLimitError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TimeSpan.FromMinutes(15));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Be("Payment rate limit exceeded");
        }

        [Fact]
        public async Task Handle_WithTransactionFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSpan?)null);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure("Transaction failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Be("An unexpected error occurred");
        }

        [Fact]
        public async Task Handle_WithCommitFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            var payments = new Dictionary<Guid, Payment>
            {
                { paymentIds[0], CreateTestPayment(paymentIds[0], clientId) }
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSpan?)null);

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdsForClientAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _retryServiceMock.Setup(x => x.GetActiveRetriesForPaymentsAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, PaymentRetrySchedule>());

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _retryServiceMock.Setup(x => x.ScheduleRetryAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentRetrySchedule.Create(TenantId.New(), Guid.NewGuid(), Guid.NewGuid(), 1));

            _unitOfWorkMock.Setup(x => x.Payments.RecordPaymentAttemptsAsync(clientId, 1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure("Commit failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Message.Should().Be("An unexpected error occurred");
        }

        [Fact]
        public async Task Handle_WithCancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSpan?)null);

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdsForClientAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Payment>());

            _retryServiceMock.Setup(x => x.GetActiveRetriesForPaymentsAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, PaymentRetrySchedule>());

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }

        [Fact]
        public async Task Handle_WithPartialSuccess_ShouldReturnMixedResults()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var paymentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new BulkRetryPaymentsCommand
            {
                ClientId = clientId,
                PaymentIds = paymentIds,
                Reason = "Manual retry"
            };

            var payments = new Dictionary<Guid, Payment>
            {
                { paymentIds[0], CreateTestPayment(paymentIds[0], clientId) },
                { paymentIds[1], CreateTestPayment(paymentIds[1], clientId) }
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _unitOfWorkMock.Setup(x => x.Payments.GetRateLimitDelayAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSpan?)null);

            _unitOfWorkMock.Setup(x => x.Payments.GetByIdsForClientAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _retryServiceMock.Setup(x => x.GetActiveRetriesForPaymentsAsync(paymentIds, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, PaymentRetrySchedule>());

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _retryServiceMock.Setup(x => x.ScheduleRetryAsync(paymentIds[0], clientId, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentRetrySchedule.Create(TenantId.New(), clientId, paymentIds[0], 1));

            _retryServiceMock.Setup(x => x.ScheduleRetryAsync(paymentIds[1], clientId, 1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Retry failed"));

            _unitOfWorkMock.Setup(x => x.Payments.RecordPaymentAttemptsAsync(clientId, 1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Value.TotalProcessed.Should().Be(2);
            result.Value.SuccessfulRetries.Should().Be(1);
            result.Value.FailedRetries.Should().Be(1);
            result.Value.Results.Should().HaveCount(2);
            result.Value.Results.Count(r => r.Success).Should().Be(1);
            result.Value.Results.Count(r => !r.Success).Should().Be(1);
        }

        #endregion

        #region Helper Methods

        private Payment CreateTestPayment(Guid paymentId, Guid clientId)
        {
            var payment = Payment.Create(
                TenantId.New(),
                Guid.NewGuid(),
                clientId,
                Money.Create(100, "USD"));
            
            // Set status to Failed so it can be retried
            payment.MarkAsFailed("Test failure");
            
            return payment;
        }

        #endregion
    }
}
