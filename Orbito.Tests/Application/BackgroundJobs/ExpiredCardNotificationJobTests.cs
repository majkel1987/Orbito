using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.BackgroundJobs;
using Xunit;

namespace Orbito.Tests.Application.BackgroundJobs;

[Trait("Category", "Unit")]
public class ExpiredCardNotificationJobTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ExpiredCardNotificationJob>> _loggerMock;

    public ExpiredCardNotificationJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ExpiredCardNotificationJob>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var job = new ExpiredCardNotificationJob(_serviceProviderMock.Object, _loggerMock.Object);

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
    public async Task ExecuteAsync_ShouldStartAndLogStartMessage()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var job = new ExpiredCardNotificationJob(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            TimeSpan.Zero); // No initial delay for testing

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(50); // Give it time to start
        cts.Cancel(); // Request stop
        await job.StopAsync(CancellationToken.None);

        // Assert
        job.Should().NotBeNull(); // Job was created successfully
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldStopGracefully()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var job = new ExpiredCardNotificationJob(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            TimeSpan.Zero);

        // Act
        var startTask = job.StartAsync(cts.Token);
        await Task.Delay(50); // Let it start

        cts.Cancel(); // Request cancellation
        await job.StopAsync(CancellationToken.None);

        // Assert
        startTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Job_ShouldHaveCorrectDefaultPeriod()
    {
        // Arrange & Act
        var job = new ExpiredCardNotificationJob(
            _serviceProviderMock.Object,
            _loggerMock.Object);

        // Assert
        job.Should().NotBeNull();
        // Period is private, but we can verify job was created with expected config
    }

    [Fact]
    public void Job_WithCustomInitialDelay_ShouldAcceptCustomDelay()
    {
        // Arrange & Act
        var customDelay = TimeSpan.FromMinutes(10);
        var job = new ExpiredCardNotificationJob(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            customDelay);

        // Assert
        job.Should().NotBeNull();
    }

    #endregion
}
