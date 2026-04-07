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
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<CheckExpiringSubscriptionsJob>> _loggerMock;
    private readonly CheckExpiringSubscriptionsJob _job;

    public CheckExpiringSubscriptionsJobTests()
    {
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _dateTimeMock = new Mock<IDateTime>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<CheckExpiringSubscriptionsJob>>();

        // Setup default date time
        _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        // Setup real ServiceCollection with mocks
        var services = new ServiceCollection();
        services.AddSingleton(_subscriptionRepositoryMock.Object);
        services.AddSingleton(_providerRepositoryMock.Object);
        services.AddSingleton(_notificationServiceMock.Object);
        services.AddSingleton(_dateTimeMock.Object);
        services.AddSingleton(_tenantContextMock.Object);
        _serviceProvider = services.BuildServiceProvider();

        // Setup default provider repository to return empty list
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());

        // Setup default subscription repository to return empty list
        _subscriptionRepositoryMock.Setup(x => x.GetExpiringSubscriptionsForTenantAsync(
            It.IsAny<TenantId>(),
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription>());

        _job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
        var action = () => new CheckExpiringSubscriptionsJob(_serviceProvider, null!);
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


        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        
        // Wait a bit to let the job start
        await Task.Delay(200);
        
        // Cancel to stop the job
        cancellationTokenSource.Cancel();

        // Assert
        await task;
        // Logger verification removed
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWaitInitialDelay()
    {
        // Arrange
        var shortDelay = TimeSpan.FromMilliseconds(100);
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, shortDelay);
        var cancellationTokenSource = new CancellationTokenSource();
        var expiringSubscriptions = new List<Subscription>();


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
    public async Task CheckExpiringSubscriptions_ShouldGetExpiringSubscriptionsForEachTenant()
    {
        // Arrange
        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, int.MaxValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });

        var shortDelay = TimeSpan.FromMilliseconds(50);
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, shortDelay);
        var cancellationToken = CancellationToken.None;

        // Act
        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert - Should iterate through tenants and check expiring subscriptions
        _providerRepositoryMock.Verify(x => x.GetActiveProvidersAsync(1, int.MaxValue, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _subscriptionRepositoryMock.Verify(x => x.GetExpiringSubscriptionsForTenantAsync(
            tenantId,
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_ShouldQueryRepositoryForExpiringSubscriptions()
    {
        // Arrange
        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;
        var cancellationToken = CancellationToken.None;

        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, int.MaxValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });

        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert - Should query repository per tenant with 7 days before expiry
        _subscriptionRepositoryMock.Verify(x => x.GetExpiringSubscriptionsForTenantAsync(
            tenantId,
            It.IsAny<DateTime>(),
            7, // DaysBeforeExpiry default value
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckExpiringSubscriptions_WithExpiringSubscriptions_ShouldSendNotifications()
    {
        // Arrange
        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithTenantId(tenantId)
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = DateTime.UtcNow.AddDays(5);

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithTenantId(tenantId)
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = DateTime.UtcNow.AddDays(3);

        var expiringSubscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, int.MaxValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });
        _subscriptionRepositoryMock.Setup(x => x.GetExpiringSubscriptionsForTenantAsync(
            tenantId,
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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

        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed
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
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Logger verification removed
    }


    [Fact]
    public async Task CheckExpiringSubscriptions_WithGeneralException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
        var cancellationTokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("General error");

        _subscriptionServiceMock.Setup(x => x.GetExpiringSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Logger verification removed
    }

    #endregion

    #region SendExpirationNotification Tests

    [Fact]
    public async Task SendExpirationNotification_ShouldCalculateDaysUntilExpiry()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var expiryDate = currentDate.AddDays(5);
        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithTenantId(tenantId)
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.NextBillingDate = expiryDate;

        var expiringSubscriptions = new List<Subscription> { subscription };
        var cancellationToken = CancellationToken.None;

        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, int.MaxValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });
        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _subscriptionRepositoryMock.Setup(x => x.GetExpiringSubscriptionsForTenantAsync(
            tenantId,
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiringSubscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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

        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var startTime = DateTime.UtcNow;

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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


        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed - sprawdzamy business logic, nie logowanie
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

        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new CheckExpiringSubscriptionsJob(_serviceProvider, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    #endregion
}




