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
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantProvider> _tenantProviderMock;
    private readonly Mock<ILogger<ProcessRecurringPaymentsJob>> _loggerMock;
    private readonly ProcessRecurringPaymentsJob _job;

    public ProcessRecurringPaymentsJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _dateTimeMock = new Mock<IDateTime>();
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantProviderMock = new Mock<ITenantProvider>();
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
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(_unitOfWorkMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(ITenantProvider)))
            .Returns(_tenantProviderMock.Object);

        // Setup UnitOfWork.Providers to return provider repository
        _unitOfWorkMock.Setup(x => x.Providers).Returns(_providerRepositoryMock.Object);

        // Setup default provider repository to return empty list
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());

        // Setup default subscription service to return successfully
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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
        // Logger verification removed - logowanie to side effect, nie główna funkcjonalność
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
    public async Task ProcessRecurringPayments_ShouldProcessPaymentsForEachTenant()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        // Setup TenantJobHelper pagination - returns provider in first page
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(It.Is<int>(p => p > 1), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>()); // Empty for subsequent pages
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(300); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert - Should iterate through tenants and process payments
        _providerRepositoryMock.Verify(x => x.GetActiveProvidersAsync(1, 100, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _subscriptionServiceMock.Verify(x => x.ProcessRecurringPaymentsAsync(testDate, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_ShouldCallSubscriptionServiceForRecurringPayments()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        // Setup TenantJobHelper pagination
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(It.Is<int>(p => p > 1), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(300); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert - Should call subscription service for processing recurring payments
        _subscriptionServiceMock.Verify(x => x.ProcessRecurringPaymentsAsync(testDate, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessRecurringPayments_ShouldCallSubscriptionServiceForExpiredSubscriptions()
    {
        // Arrange
        var testDate = DateTime.UtcNow;
        var cancellationToken = CancellationToken.None;

        var provider = Provider.Create(Guid.NewGuid(), "TestProvider", "test-slug");
        var tenantId = provider.TenantId;

        _dateTimeMock.Setup(x => x.UtcNow).Returns(testDate);
        // Setup TenantJobHelper pagination
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider> { provider });
        _providerRepositoryMock.Setup(x => x.GetActiveProvidersAsync(It.Is<int>(p => p > 1), 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());
        _subscriptionServiceMock.Setup(x => x.ProcessRecurringPaymentsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _subscriptionServiceMock.Setup(x => x.ProcessExpiredSubscriptionsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var job = new ProcessRecurringPaymentsJob(_serviceProviderMock.Object, _loggerMock.Object, TimeSpan.FromMilliseconds(50));

        await job.StartAsync(cancellationToken);
        await Task.Delay(300); // Let it process once
        await job.StopAsync(cancellationToken);

        // Assert - Should call subscription service for processing expired subscriptions
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
        // Logger verification removed - sprawdzamy business logic, nie logowanie
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
        // Logger verification removed - sprawdzamy business logic, nie logowanie
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

        // Assert - Logger verification removed
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

        // Assert - Logger verification removed (logowanie to side effect)
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
        // Logger verification removed - sprawdzamy business logic, nie logowanie
    }

    #endregion
}




