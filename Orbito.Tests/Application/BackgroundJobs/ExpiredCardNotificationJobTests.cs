using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.BackgroundJobs;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;
using Orbito.Tests.Helpers;
using Orbito.Tests.Helpers.TestDataBuilders;
using Xunit;

namespace Orbito.Tests.Application.BackgroundJobs;

[Trait("Category", "Unit")]
public class ExpiredCardNotificationJobTests : BaseTestFixture
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<ExpiredCardNotificationJob>> _loggerMock;
    private readonly ExpiredCardNotificationJob _job;

    public ExpiredCardNotificationJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _dateTimeMock = new Mock<IDateTime>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<ExpiredCardNotificationJob>>();

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
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPaymentNotificationService)))
            .Returns(_notificationServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IDateTime)))
            .Returns(_dateTimeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ITenantContext)))
            .Returns(_tenantContextMock.Object);

        _job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ExpiredCardNotificationJob(null!, _loggerMock.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ExpiredCardNotificationJob(_serviceProviderMock.Object, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_ShouldStartAndStopJob()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        
        // Wait a bit to let the job start
        await Task.Delay(200);
        
        // Cancel to stop the job
        cancellationTokenSource.Cancel();

        // Assert
        await task;
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ExpiredCardNotificationJob started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWaitInitialDelay()
    {
        // Arrange
        var shortDelay = TimeSpan.FromMilliseconds(100);
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, shortDelay);
        var cancellationTokenSource = new CancellationTokenSource();
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        var startTime = DateTime.UtcNow;

        // Act


        var task = job.StartAsync(cancellationTokenSource.Token);
        
        // Wait a bit to let the job start
        await Task.Delay(200);
        
        // Cancel to stop the job
        cancellationTokenSource.Cancel();

        // Assert
        await task;
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeGreaterThan(shortDelay); // Should wait at least the initial delay
    }

    #endregion

    #region ProcessExpiredCardsAsync Tests

    [Fact]
    public async Task ProcessExpiredCardsAsync_ShouldSetAdminTenantContext()
    {
        // Arrange
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _tenantContextMock.Verify(x => x.SetTenant(null), Times.AtLeastOnce);
        _tenantContextMock.Verify(x => x.ClearTenant(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiredCardsAsync_ShouldGetExpiredPaymentMethods()
    {
        // Arrange
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiredCardsAsync_WithExpiredCards_ShouldSendNotifications()
    {
        // Arrange
        var expiredCard1 = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(DateTime.UtcNow.AddDays(-1)) // Expired yesterday
            .Build();

        var expiredCard2 = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(DateTime.UtcNow.AddDays(-5)) // Expired 5 days ago
            .Build();

        var expiredPaymentMethods = new List<PaymentMethod> { expiredCard1, expiredCard2 };
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);
        _notificationServiceMock.Setup(x => x.SendExpiredCardNotificationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _notificationServiceMock.Verify(x => x.SendExpiredCardNotificationAsync(
            expiredCard1.Id, 
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _notificationServiceMock.Verify(x => x.SendExpiredCardNotificationAsync(
            expiredCard2.Id, 
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiredCardsAsync_WithNotificationFailure_ShouldLogError()
    {
        // Arrange
        var expiredCard = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(DateTime.UtcNow.AddDays(-1))
            .Build();

        var expiredPaymentMethods = new List<PaymentMethod> { expiredCard };
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Notification failed");

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);
        _notificationServiceMock.Setup(x => x.SendExpiredCardNotificationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send expired card notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region ProcessExpiringCardsAsync Tests

    [Fact]
    public async Task ProcessExpiringCardsAsync_ShouldGetPaymentMethodsByType()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiryThresholdDate = currentDate.AddDays(30);
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.PaymentMethods.GetByTypeAsync(
            PaymentMethodType.Card, 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiringCardsAsync_WithExpiringCards_ShouldSendNotifications()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiryThresholdDate = currentDate.AddDays(30);
        
        var expiringCard1 = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(currentDate.AddDays(15)) // Expires in 15 days
            .Build();

        var expiringCard2 = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(currentDate.AddDays(25)) // Expires in 25 days
            .Build();

        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod> { expiringCard1, expiringCard2 };
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);
        _notificationServiceMock.Setup(x => x.SendCardExpiringSoonNotificationAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _notificationServiceMock.Verify(x => x.SendCardExpiringSoonNotificationAsync(
            expiringCard1.Id, 
            15, // 15 days until expiry
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _notificationServiceMock.Verify(x => x.SendCardExpiringSoonNotificationAsync(
            expiringCard2.Id, 
            25, // 25 days until expiry
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiringCardsAsync_ShouldProcessInBatches()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.PaymentMethods.GetByTypeAsync(
            PaymentMethodType.Card, 
            1, // First page
            100, // Page size
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiringCardsAsync_WithNotificationFailure_ShouldLogError()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiringCard = PaymentMethodTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithType(PaymentMethodType.Card)
            .WithExpiryDate(currentDate.AddDays(15))
            .Build();

        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod> { expiringCard };
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Notification failed");

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);
        _notificationServiceMock.Setup(x => x.SendCardExpiringSoonNotificationAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send expiring card notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Timeout and Error Handling Tests

    [Fact]
    public async Task ProcessExpiredCardsAsync_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                // Simulate long operation that gets cancelled
                await Task.Delay(TimeSpan.FromMinutes(25), ct); 
                return new List<PaymentMethod>();
            });
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod>());

        // Act
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Should log cancellation warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("operation was cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiringCardsAsync_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod>());
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(async (PaymentMethodType type, int page, int size, CancellationToken ct) =>
            {
                // Simulate long operation that gets cancelled
                await Task.Delay(TimeSpan.FromMinutes(25), ct); 
                return new List<PaymentMethod>();
            });

        // Act
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Should log cancellation warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("operation was cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                return new List<PaymentMethod>();
            });
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentMethod>());

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Let it start
        cancellationTokenSource.Cancel();
        await task;

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("operation was cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WithGeneralException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
        var cancellationTokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("General error");

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Should log error but not throw exception
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while processing expired/expiring cards")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ProcessExpiredCardsAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing expired payment cards")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed expired card notification processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessExpiringCardsAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiryThresholdDate = currentDate.AddDays(30);
        var expiredPaymentMethods = new List<PaymentMethod>();
        var paymentMethods = new List<PaymentMethod>();
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetExpiredPaymentMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredPaymentMethods);
        _unitOfWorkMock.Setup(x => x.PaymentMethods.GetByTypeAsync(It.IsAny<PaymentMethodType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentMethods);

        // Act

        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing payment cards expiring before")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed expiring card notification processing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}




