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
    public class CancelRetryCommandHandlerTests
    {
        private readonly Mock<IPaymentRetryRepository> _retryRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILogger<CancelRetryCommandHandler>> _loggerMock;
        private readonly CancelRetryCommandHandler _handler;

        public CancelRetryCommandHandlerTests()
        {
            _retryRepositoryMock = new Mock<IPaymentRetryRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggerMock = new Mock<ILogger<CancelRetryCommandHandler>>();

            _handler = new CancelRetryCommandHandler(
                _retryRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _userContextServiceMock.Object,
                _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var handler = new CancelRetryCommandHandler(
                _retryRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _userContextServiceMock.Object,
                _loggerMock.Object);

            // Assert
            handler.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullRetryRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CancelRetryCommandHandler(
                    null!,
                    _unitOfWorkMock.Object,
                    _userContextServiceMock.Object,
                    _loggerMock.Object));

            exception.ParamName.Should().Be("retryRepository");
        }

        [Fact]
        public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CancelRetryCommandHandler(
                    _retryRepositoryMock.Object,
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
                new CancelRetryCommandHandler(
                    _retryRepositoryMock.Object,
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
                new CancelRetryCommandHandler(
                    _retryRepositoryMock.Object,
                    _unitOfWorkMock.Object,
                    _userContextServiceMock.Object,
                    null!));

            exception.ParamName.Should().Be("logger");
        }

        #endregion

        #region Handle Tests

        [Fact]
        public async Task Handle_WithValidRequest_ShouldCancelRetry()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            var retrySchedule = CreateTestRetrySchedule(scheduleId, clientId, RetryStatus.Scheduled);

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _retryRepositoryMock.Setup(x => x.GetByIdForClientAsync(scheduleId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(retrySchedule);

            _retryRepositoryMock.Setup(x => x.UpdateAsync(retrySchedule, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Success(1));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ScheduleId.Should().Be(scheduleId);
            retrySchedule.Status.Should().Be(RetryStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_WithDifferentClientId_ShouldReturnSecurityError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var differentClientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(differentClientId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("You can only cancel your own retry schedules");
        }

        [Fact]
        public async Task Handle_WithNonExistentSchedule_ShouldReturnNotFoundError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _retryRepositoryMock.Setup(x => x.GetByIdForClientAsync(scheduleId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PaymentRetrySchedule?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Retry schedule not found");
        }

        [Fact]
        public async Task Handle_WithCompletedSchedule_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            var retrySchedule = CreateTestRetrySchedule(scheduleId, clientId, RetryStatus.Completed);

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _retryRepositoryMock.Setup(x => x.GetByIdForClientAsync(scheduleId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(retrySchedule);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot cancel a completed retry schedule");
        }

        [Fact]
        public async Task Handle_WithAlreadyCancelledSchedule_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            var retrySchedule = CreateTestRetrySchedule(scheduleId, clientId, RetryStatus.Cancelled);

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _retryRepositoryMock.Setup(x => x.GetByIdForClientAsync(scheduleId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(retrySchedule);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Retry schedule is already cancelled");
        }

        [Fact]
        public async Task Handle_WithSaveChangesFailure_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            var retrySchedule = CreateTestRetrySchedule(scheduleId, clientId, RetryStatus.Scheduled);

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId);

            _retryRepositoryMock.Setup(x => x.GetByIdForClientAsync(scheduleId, clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(retrySchedule);

            _retryRepositoryMock.Setup(x => x.UpdateAsync(retrySchedule, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<int>.Failure("Save failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred while cancelling the retry schedule");
        }

        [Fact]
        public async Task Handle_WithException_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("An error occurred while cancelling the retry schedule");
        }

        [Fact]
        public async Task Handle_WithCancellation_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var command = new CancelRetryCommand
            {
                ClientId = clientId,
                ScheduleId = scheduleId
            };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _handler.Handle(command, cancellationTokenSource.Token));
        }

        #endregion

        #region Helper Methods

        private PaymentRetrySchedule CreateTestRetrySchedule(Guid scheduleId, Guid clientId, RetryStatus status)
        {
            return PaymentRetrySchedule.Create(
                TenantId.New(),
                clientId,
                Guid.NewGuid(),
                1);
        }

        #endregion
    }
}
