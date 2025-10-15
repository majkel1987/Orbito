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
public class CheckExpiringSubscriptionsJobTests : BaseTestFixture
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CheckExpiringSubscriptionsJob>> _loggerMock;
    private readonly CheckExpiringSubscriptionsJob _job;

    public CheckExpiringSubscriptionsJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _dateTimeMock = new Mock<IDateTime>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CheckExpiringSubscriptionsJob>>();

        // Setup service provider chain
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup service resolution
        _serviceProviderMock.Setup(x => x.GetService(typeof(ISubscriptionService)))
            .Returns(_subscriptionServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPaymentNotificationService)))
            .Returns(_notificationServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IDateTime)))
            .Returns(_dateTimeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ITenantContext)))
            .Returns(_tenantContextMock.Object);

        _job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new CheckExpiringSubscriptionsJob(null!, _loggerMock.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, null!);
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
        var expiringSubscriptions = new List<Subscription>();

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CheckExpiringSubscriptionsJob started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWaitInitialDelay()
    {
        // Arrange
        var shortDelay = TimeSpan.FromMilliseconds(100);
        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, shortDelay);
        var cancellationTokenSource = new CancellationTokenSource();
        var expiringSubscriptions = new List<Subscription>();

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);

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

    #region CheckExpiringSubscriptions Tests

    [Fact]
    public async Task CheckExpiringSubscriptions_ShouldSetAdminTenantContext()
    {
        // Arrange
        var shortDelay = TimeSpan.FromMilliseconds(50);
        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, shortDelay);
        var expiringSubscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);

        // Act
        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _tenantContextMock.Verify(x => x.SetTenant(null), Times.AtLeastOnce);
        _tenantContextMock.Verify(x => x.ClearTenant(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_ShouldGetExpiringSubscriptions()
    {
        // Arrange
        var expiringSubscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _subscriptionServiceMock.Verify(x => x.GetExpiringSubscriptionsAsync(7, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_WithExpiringSubscriptions_ShouldSendNotifications()
    {
        // Arrange
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = DateTime.UtcNow.AddDays(5);

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = DateTime.UtcNow.AddDays(3);

        var expiringSubscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _notificationServiceMock.Verify(x => x.SendUpcomingPaymentReminderAsync(
            subscription1.Id, 
            It.IsAny<int>(), 
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _notificationServiceMock.Verify(x => x.SendUpcomingPaymentReminderAsync(
            subscription2.Id, 
            It.IsAny<int>(), 
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_WithNotificationFailure_ShouldLogError()
    {
        // Arrange
        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.NextBillingDate = DateTime.UtcNow.AddDays(5);

        var expiringSubscriptions = new List<Subscription> { subscription };
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Notification failed");

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send expiration notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(async (int days, CancellationToken ct) =>
            {
                // Simulate long operation that gets cancelled
                await Task.Delay(TimeSpan.FromMinutes(15), ct); 
                return new List<Subscription>();
            });

        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
    public async Task CheckExpiringSubscriptions_WithGeneralException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
        var cancellationTokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("General error");

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while checking expiring subscriptions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region SendExpirationNotification Tests

    [Fact]
    public async Task SendExpirationNotification_ShouldCalculateDaysUntilExpiry()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiryDate = currentDate.AddDays(5);
        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.NextBillingDate = expiryDate;

        var expiringSubscriptions = new List<Subscription> { subscription };
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _notificationServiceMock.Verify(x => x.SendUpcomingPaymentReminderAsync(
            subscription.Id, 
            5, // Should calculate 5 days until expiry
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendExpirationNotification_ShouldAddDelayBetweenNotifications()
    {
        // Arrange
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = DateTime.UtcNow.AddDays(5);

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = DateTime.UtcNow.AddDays(3);

        var expiringSubscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var startTime = DateTime.UtcNow;

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(100)); // Should have delay between notifications
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task CheckExpiringSubscriptions_ShouldLogStartAndCompletion()
    {
        // Arrange
        var expiringSubscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Checking for subscriptions expiring within 7 days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed expiring subscription check")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_ShouldLogFoundSubscriptionsCount()
    {
        // Arrange
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = DateTime.UtcNow.AddDays(5);

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = DateTime.UtcNow.AddDays(3);

        var expiringSubscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found 2 subscriptions expiring within 7 days")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}




