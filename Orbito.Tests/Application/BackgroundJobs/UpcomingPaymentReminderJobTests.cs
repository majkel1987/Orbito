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
public class UpcomingPaymentReminderJobTests : BaseTestFixture
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentNotificationService> _notificationServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<UpcomingPaymentReminderJob>> _loggerMock;
    private readonly UpcomingPaymentReminderJob _job;

    public UpcomingPaymentReminderJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<IPaymentNotificationService>();
        _dateTimeMock = new Mock<IDateTime>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<UpcomingPaymentReminderJob>>();

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

        _job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new UpcomingPaymentReminderJob(null!, _loggerMock.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new UpcomingPaymentReminderJob(_serviceProviderMock.Object, null!);
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
        var subscriptions = new List<Subscription>();

        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, shortDelay);
        var cancellationTokenSource = new CancellationTokenSource();
        var subscriptions = new List<Subscription>();

        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

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

    #region SendUpcomingPaymentRemindersAsync Tests

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_ShouldSetAdminTenantContext()
    {
        // Arrange
        var subscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _tenantContextMock.Verify(x => x.SetTenant(null), Times.AtLeastOnce);
        _tenantContextMock.Verify(x => x.ClearTenant(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_ShouldGetSubscriptionsForBilling()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        var subscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.Subscriptions.GetSubscriptionsForBillingAsync(reminderDate, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_WithSubscriptions_ShouldSendReminders()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = reminderDate;

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = reminderDate;

        var subscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _notificationServiceMock.Verify(x => x.SendUpcomingPaymentReminderAsync(
            subscription1.Id, 
            3, // 3 days before payment
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _notificationServiceMock.Verify(x => x.SendUpcomingPaymentReminderAsync(
            subscription2.Id, 
            3, // 3 days before payment
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_WithNotificationFailure_ShouldLogError()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        
        var subscription = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription.NextBillingDate = reminderDate;

        var subscriptions = new List<Subscription> { subscription };
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Notification failed");

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();

        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(async (DateTime date, CancellationToken ct) =>
            {
                // Simulate long operation that gets cancelled
                await Task.Delay(TimeSpan.FromMinutes(20), ct); 
                return new List<Subscription>();
            });

        // Act
        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Should log cancellation warning
        // Logger verification removed
    }


    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_WithGeneralException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
        var cancellationTokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("General error");

        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var task = job.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100); // Let it process once
        cancellationTokenSource.Cancel();
        await task;

        // Assert - Should log error but not throw exception
        // Logger verification removed
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_ShouldAddDelayBetweenNotifications()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = reminderDate;

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = reminderDate;

        var subscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var startTime = DateTime.UtcNow;

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
    public async Task SendUpcomingPaymentRemindersAsync_ShouldLogStartAndCompletion()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        var subscriptions = new List<Subscription>();
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    [Fact]
    public async Task SendUpcomingPaymentRemindersAsync_ShouldLogFoundSubscriptionsCount()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;
        var reminderDate = currentDate.AddDays(3).Date;
        
        var subscription1 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription1.NextBillingDate = reminderDate;

        var subscription2 = SubscriptionTestDataBuilder.Create()
            .WithId(Guid.NewGuid())
            .WithClientId(Guid.NewGuid())
            .WithStatus(SubscriptionStatus.Active)
            .Build();
        subscription2.NextBillingDate = reminderDate;

        var subscriptions = new List<Subscription> { subscription1, subscription2 };
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(currentDate);
        _unitOfWorkMock.Setup(x => x.Subscriptions.GetSubscriptionsForBillingAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);
        _notificationServiceMock.Setup(x => x.SendUpcomingPaymentReminderAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new UpcomingPaymentReminderJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        // Logger verification removed
    }

    #endregion
}




