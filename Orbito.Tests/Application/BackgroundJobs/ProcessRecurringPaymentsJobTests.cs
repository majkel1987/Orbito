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
public class ProcessRecurringPaymentsJobTests : BaseTestFixture
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILogger<ProcessRecurringPaymentsJob>> _loggerMock;
    private readonly ProcessRecurringPaymentsJob _job;

    public ProcessRecurringPaymentsJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _dateTimeMock = new Mock<IDateTime>();
        _tenantContextMock = new Mock<ITenantContext>();
        _loggerMock = new Mock<ILogger<ProcessRecurringPaymentsJob>>();

        // Setup service provider chain
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup service resolution - use GetService to avoid extension method issues
        _serviceProviderMock.Setup(x => x.GetService(typeof(ISubscriptionService)))
            .Returns(_subscriptionServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IDateTime)))
            .Returns(_dateTimeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ITenantContext)))
            .Returns(_tenantContextMock.Object);

        _job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessRecurringPaymentsJob(null!, _loggerMock.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, null!);
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
        var testDate = DateTime.UtcNow;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ProcessRecurringPaymentsJob started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldWaitInitialDelay()
    {
        // Arrange
        var shortDelay = TimeSpan.FromMilliseconds(100);
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, shortDelay);
        var cancellationTokenSource = new CancellationTokenSource();
        var testDate = DateTime.UtcNow;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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

    #region ProcessRecurringPayments Tests

    [Fact]
    public async Task ProcessRecurringPayments_ShouldSetAdminTenantContext()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _tenantContextMock.Verify(x => x.SetTenant(null), Times.AtLeastOnce);
        _tenantContextMock.Verify(x => x.ClearTenant(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_ShouldProcessRecurringPayments()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _subscriptionServiceMock.Verify(x => x.ProcessRecurringPaymentsAsync(testDate, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_ShouldProcessExpiredSubscriptions()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _subscriptionServiceMock.Verify(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_WithRecurringPaymentsFailure_ShouldLogError()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Recurring payments failed");

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process recurring payments")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_WithExpiredSubscriptionsFailure_ShouldLogError()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;
        var exception = new InvalidOperationException("Expired subscriptions failed");

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process expired subscriptions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_WithCancellation_ShouldLogCancellationWarning()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationTokenSource = new CancellationTokenSource();

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(async (DateTime date, CancellationToken ct) =>
            {
                // Simulate long operation that gets cancelled
                await Task.Delay(TimeSpan.FromMinutes(20), ct); 
            });
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

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
    public async Task ProcessRecurringPayments_WithGeneralException_ShouldLogErrorAndContinue()
    {
        // Arrange
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));
        var testDate = DateTime.UtcNow;
        var cancellationTokenSource = new CancellationTokenSource();
        var exception = new InvalidOperationException("General error");

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while processing recurring payments")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ProcessRecurringPayments_ShouldLogStartAndCompletion()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(200); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing recurring payments for date")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed recurring payments job")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}




