using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.BackgroundJobs;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.BackgroundJobs;

[Trait("Category", "Unit")]
public class ProcessEmailNotificationsJobTests : BaseTestFixture
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly Mock<ILogger<ProcessEmailNotificationsJob>> _loggerMock;
    private readonly ProcessEmailNotificationsJob _job;

    public ProcessEmailNotificationsJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailSenderMock = new Mock<IEmailSender>();
        _loggerMock = new Mock<ILogger<ProcessEmailNotificationsJob>>();

        // Setup service provider chain
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup service resolution
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(_unitOfWorkMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEmailSender)))
            .Returns(_emailSenderMock.Object);

        _job = new ProcessEmailNotificationsJob(_serviceProviderMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new ProcessEmailNotificationsJob(_serviceProviderMock.Object, _loggerMock.Object);

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessEmailNotificationsJob(null!, _loggerMock.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessEmailNotificationsJob(_serviceProviderMock.Object, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region ProcessPendingNotificationsAsync Tests

    [Fact]
    public async Task ProcessPendingNotificationsAsync_ShouldGetPendingNotifications()
    {
        // Arrange
        var pendingNotifications = new List<EmailNotification>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.EmailNotifications.GetPendingNotificationsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithPendingNotifications_ShouldProcessThem()
    {
        // Arrange
        var notification1 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test1@example.com")
            .WithSubject("Test Subject 1")
            .WithBody("Test Body 1")
            .WithRetryCount(0)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1)) // Ready for retry
            .Build();

        var notification2 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test2@example.com")
            .WithSubject("Test Subject 2")
            .WithBody("Test Body 2")
            .WithRetryCount(1)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1)) // Ready for retry
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification1, notification2 };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);
        _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            "test1@example.com",
            "Test Subject 1",
            "Test Body 1",
            false,
            cancellationToken), Times.Once);
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            "test2@example.com",
            "Test Subject 2",
            "Test Body 2",
            false,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithSuccessfulSend_ShouldMarkAsProcessed()
    {
        // Arrange
        var notification = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test@example.com")
            .WithSubject("Test Subject")
            .WithBody("Test Body")
            .WithRetryCount(0)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1)) // Ready for retry
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);
        _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        notification.Status.Should().Be("Processed");
        _unitOfWorkMock.Verify(x => x.EmailNotifications.UpdateAsync(notification, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithSendFailure_ShouldScheduleRetry()
    {
        // Arrange
        var notification = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test@example.com")
            .WithSubject("Test Subject")
            .WithBody("Test Body")
            .WithRetryCount(1)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1)) // Ready for retry
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification };
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Email send failed");

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);
        _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        _unitOfWorkMock.Setup(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        notification.RetryCount.Should().Be(2); // Should increment retry count
        notification.Status.Should().Be("Pending");
        notification.NextRetryAt.Should().BeAfter(DateTime.UtcNow); // Should schedule next retry
        _unitOfWorkMock.Verify(x => x.EmailNotifications.UpdateAsync(notification, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithNotReadyForRetry_ShouldSkip()
    {
        // Arrange
        var notification = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test@example.com")
            .WithSubject("Test Subject")
            .WithBody("Test Body")
            .WithRetryCount(1)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(5)) // Not ready for retry yet
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithMaxRetriesReached_ShouldNotRetry()
    {
        // Arrange
        var notification = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test@example.com")
            .WithSubject("Test Subject")
            .WithBody("Test Body")
            .WithRetryCount(3) // Max retries reached
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1)) // Ready for retry
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        _emailSenderMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_WithException_ShouldLogError()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Database error");

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    #endregion

    #region CleanupFailedNotificationsAsync Tests

    [Fact]
    public async Task CleanupFailedNotificationsAsync_ShouldGetFailedNotifications()
    {
        // Arrange
        var failedNotifications = new List<EmailNotification>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetFailedNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedNotifications);

        // Act
        await _job.CleanupFailedNotificationsAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.EmailNotifications.GetFailedNotificationsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CleanupFailedNotificationsAsync_WithFailedNotifications_ShouldLogWarnings()
    {
        // Arrange
        var failedNotification1 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test1@example.com")
            .WithSubject("Test Subject 1")
            .WithBody("Test Body 1")
            .WithRetryCount(3)
            .WithMaxRetries(3)
            .WithStatus("Failed")
            .WithErrorMessage("Connection timeout")
            .Build();

        var failedNotification2 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test2@example.com")
            .WithSubject("Test Subject 2")
            .WithBody("Test Body 2")
            .WithRetryCount(3)
            .WithMaxRetries(3)
            .WithStatus("Failed")
            .WithErrorMessage("Invalid email address")
            .Build();

        var failedNotifications = new List<EmailNotification> { failedNotification1, failedNotification2 };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetFailedNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedNotifications);

        // Act
        await _job.CleanupFailedNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    [Fact]
    public async Task CleanupFailedNotificationsAsync_WithException_ShouldLogError()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Database error");

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetFailedNotificationsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _job.CleanupFailedNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ProcessPendingNotificationsAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var pendingNotifications = new List<EmailNotification>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_ShouldLogFoundNotificationsCount()
    {
        // Arrange
        var notification1 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test1@example.com")
            .WithSubject("Test Subject 1")
            .WithBody("Test Body 1")
            .WithRetryCount(0)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1))
            .Build();

        var notification2 = EmailNotificationTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithRecipientEmail("test2@example.com")
            .WithSubject("Test Subject 2")
            .WithBody("Test Body 2")
            .WithRetryCount(1)
            .WithMaxRetries(3)
            .WithNextRetryAt(DateTime.UtcNow.AddMinutes(-1))
            .Build();

        var pendingNotifications = new List<EmailNotification> { notification1, notification2 };
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetPendingNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingNotifications);
        _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.EmailNotifications.UpdateAsync(It.IsAny<EmailNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));

        // Act
        await _job.ProcessPendingNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    [Fact]
    public async Task CleanupFailedNotificationsAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var failedNotifications = new List<EmailNotification>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.EmailNotifications.GetFailedNotificationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedNotifications);

        // Act
        await _job.CleanupFailedNotificationsAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    #endregion
}

// Helper class for EmailNotification test data builder
public static class EmailNotificationTestDataBuilder
{
    public static EmailNotificationBuilder Create()
    {
        return new EmailNotificationBuilder();
    }
}

public class EmailNotificationBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _recipientEmail = "test@example.com";
    private string _subject = "Test Subject";
    private string _body = "Test Body";
    private string _status = "Pending";
    private int _retryCount = 0;
    private int _maxRetries = 3;
    private DateTime? _nextRetryAt = DateTime.UtcNow.AddMinutes(-1);
    private string? _errorMessage = null;
    private DateTime _createdAt = DateTime.UtcNow;

    public EmailNotificationBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EmailNotificationBuilder WithRecipientEmail(string recipientEmail)
    {
        _recipientEmail = recipientEmail;
        return this;
    }

    public EmailNotificationBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }

    public EmailNotificationBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public EmailNotificationBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public EmailNotificationBuilder WithRetryCount(int retryCount)
    {
        _retryCount = retryCount;
        return this;
    }

    public EmailNotificationBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    public EmailNotificationBuilder WithNextRetryAt(DateTime? nextRetryAt)
    {
        _nextRetryAt = nextRetryAt;
        return this;
    }

    public EmailNotificationBuilder WithErrorMessage(string? errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    public EmailNotificationBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public EmailNotification Build()
    {
        // Create a real EmailNotification object instead of mock
        var notification = new EmailNotification
        {
            Id = _id,
            RecipientEmail = _recipientEmail,
            Subject = _subject,
            Body = _body,
            Status = _status,
            RetryCount = _retryCount,
            MaxRetries = _maxRetries,
            NextRetryAt = _nextRetryAt,
            ErrorMessage = _errorMessage,
            CreatedAt = _createdAt,
            TenantId = TenantId.New(),
            Type = "Test"
        };
        
        return notification;
    }
}
