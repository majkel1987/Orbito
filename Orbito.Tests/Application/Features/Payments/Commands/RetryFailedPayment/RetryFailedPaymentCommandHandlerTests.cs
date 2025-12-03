using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Features.Payments.Commands;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.Features.Payments.Commands.RetryFailedPayment;

[Trait("Category", "Unit")]
public class RetryFailedPaymentCommandHandlerTests : BaseTestFixture
{
    private readonly Mock<IPaymentRetryService> _retryServiceMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ILogger<RetryFailedPaymentCommandHandler>> _loggerMock;
    private readonly RetryFailedPaymentCommandHandler _handler;

    public RetryFailedPaymentCommandHandlerTests()
    {
        _retryServiceMock = new Mock<IPaymentRetryService>();
        _userContextServiceMock = new Mock<IUserContextService>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _loggerMock = new Mock<ILogger<RetryFailedPaymentCommandHandler>>();

        // Setup UnitOfWork to return payment repository
        UnitOfWorkMock.Setup(x => x.Payments).Returns(_paymentRepositoryMock.Object);

        _handler = new RetryFailedPaymentCommandHandler(
            _retryServiceMock.Object,
            UnitOfWorkMock.Object,
            _userContextServiceMock.Object,
            _loggerMock.Object);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidFailedPayment_ShouldScheduleRetrySuccessfully()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Manual retry request"
        };

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(clientId)
            .WithStatus(PaymentStatus.Failed)
            .Build();

        var retrySchedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithPaymentId(paymentId)
            .WithAttemptNumber(1)
            .WithMaxAttempts(3)
            .WithNextAttemptAt(DateTime.UtcNow.AddMinutes(5))
            .Build();

        // Setup mocks
        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync(payment);

        _retryServiceMock.Setup(x => x.HasActiveRetriesAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(false);

        _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(1);

        _retryServiceMock.Setup(x => x.ScheduleRetryAsync(
                paymentId,
                clientId,
                1,
                "Manual retry request",
                CancellationToken.None))
            .ReturnsAsync(retrySchedule);

        _paymentRepositoryMock.Setup(x => x.RecordPaymentAttemptAsync(clientId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Setup transaction mocks
        UnitOfWorkMock.Setup(x => x.BeginTransactionAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success());

        UnitOfWorkMock.Setup(x => x.CommitAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.RetryScheduleId.Should().Be(retrySchedule.Id);
        result.Value.NextAttemptAt.Should().Be(retrySchedule.NextAttemptAt);
        result.Value.AttemptNumber.Should().Be(1);
        result.Value.MaxAttempts.Should().Be(3);

        // Verify service calls
        _retryServiceMock.Verify(x => x.ScheduleRetryAsync(
            paymentId,
            clientId,
            1,
            "Manual retry request",
            CancellationToken.None), Times.Once);

        _paymentRepositoryMock.Verify(x => x.RecordPaymentAttemptAsync(clientId, CancellationToken.None), Times.Once);
        UnitOfWorkMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithDifferentClientId_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = TestClientId,
            Reason = "Test"
        };

        var differentClientId = Guid.NewGuid();

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(differentClientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Unauthorized access");
    }

    [Fact]
    public async Task Handle_WithRateLimitExceeded_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        var rateLimitDelay = TimeSpan.FromMinutes(15);

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync(rateLimitDelay);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Payment rate limit exceeded");
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Payment was not found");
    }

    [Fact]
    public async Task Handle_WithNonFailedPayment_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(clientId)
            .WithStatus(PaymentStatus.Completed) // Not failed
            .Build();

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Only failed payments can be retried");
    }

    [Fact]
    public async Task Handle_WithActiveRetry_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(clientId)
            .WithStatus(PaymentStatus.Failed)
            .Build();

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync(payment);

        _retryServiceMock.Setup(x => x.HasActiveRetriesAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(true); // Has active retry

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Payment already has an active retry schedule");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenTransactionBeginFails_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(clientId)
            .WithStatus(PaymentStatus.Failed)
            .Build();

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync(payment);

        _retryServiceMock.Setup(x => x.HasActiveRetriesAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(false);

        _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(1);

        // Setup transaction to fail
        UnitOfWorkMock.Setup(x => x.BeginTransactionAsync(CancellationToken.None))
            .ReturnsAsync(Result.Failure("Transaction failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("An unexpected error occurred");
    }

    [Fact]
    public async Task Handle_WhenTransactionCommitFails_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        var payment = PaymentTestDataBuilder.Create()
            .WithTenantId(TestTenantId)
            .WithClientId(clientId)
            .WithStatus(PaymentStatus.Failed)
            .Build();

        var retrySchedule = PaymentRetryScheduleTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithPaymentId(paymentId)
            .Build();

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientId);

        _paymentRepositoryMock.Setup(x => x.GetRateLimitDelayAsync(clientId, CancellationToken.None))
            .ReturnsAsync((TimeSpan?)null);

        _paymentRepositoryMock.Setup(x => x.GetByIdForClientAsync(paymentId, clientId, CancellationToken.None))
            .ReturnsAsync(payment);

        _retryServiceMock.Setup(x => x.HasActiveRetriesAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(false);

        _retryServiceMock.Setup(x => x.CalculateNextAttemptNumberAsync(paymentId, CancellationToken.None))
            .ReturnsAsync(1);

        _retryServiceMock.Setup(x => x.ScheduleRetryAsync(
                paymentId,
                clientId,
                1,
                "Test",
                CancellationToken.None))
            .ReturnsAsync(retrySchedule);

        _paymentRepositoryMock.Setup(x => x.RecordPaymentAttemptAsync(clientId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Setup transaction mocks
        UnitOfWorkMock.Setup(x => x.BeginTransactionAsync(CancellationToken.None))
            .ReturnsAsync(Result.Success());

        UnitOfWorkMock.Setup(x => x.CommitAsync(CancellationToken.None))
            .ReturnsAsync(Result.Failure("Commit failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("An unexpected error occurred");

        // Verify rollback was called
        UnitOfWorkMock.Verify(x => x.RollbackAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var clientId = TestClientId;
        var command = new RetryFailedPaymentCommand
        {
            PaymentId = paymentId,
            ClientId = clientId,
            Reason = "Test"
        };

        _userContextServiceMock.Setup(x => x.GetCurrentClientIdAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().NotBeNullOrEmpty();
        result.Error.Message.Should().Contain("An unexpected error occurred");
    }

    #endregion
}
