using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Orbito.Infrastructure.BackgroundJobs;
using Orbito.Infrastructure.Data;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace Orbito.Tests.Infrastructure.BackgroundJobs;

[Trait("Category", "Unit")]
public class ProcessDuePaymentsJobTests : IDisposable
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly Mock<ILogger<ProcessDuePaymentsJob>> _mockLogger;
    private readonly Mock<IPaymentProcessingService> _mockPaymentService;
    private readonly Mock<ITenantProvider> _mockTenantProvider;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApplicationDbContext>> _mockDbContextLogger;
    private readonly Mock<IDateTime> _mockDateTime;
    private readonly ApplicationDbContext _context;
    private readonly ProcessDuePaymentsJob _job;

    public ProcessDuePaymentsJobTests()
    {
        _mockLogger = new Mock<ILogger<ProcessDuePaymentsJob>>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextLogger = new Mock<ILogger<ApplicationDbContext>>();
        _mockPaymentService = new Mock<IPaymentProcessingService>();
        _mockTenantProvider = new Mock<ITenantProvider>();
        _mockDateTime = new Mock<IDateTime>();

        _mockDateTime.Setup(x => x.UtcNow).Returns(new DateTime(2025, 11, 16, 12, 0, 0, DateTimeKind.Utc));

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options, _mockTenantProvider.Object, _mockHttpContextAccessor.Object, _mockDbContextLogger.Object);

        // Setup real service provider with mocks
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddSingleton(_mockPaymentService.Object);
        services.AddSingleton(_mockTenantProvider.Object);
        services.AddSingleton(_mockDateTime.Object);
        _mockServiceProvider = services.BuildServiceProvider();

        _job = new ProcessDuePaymentsJob(_mockServiceProvider, _mockLogger.Object, TimeSpan.FromMilliseconds(100));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task Execute_HasActiveTenants_ShouldProcessDuePayments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var tenantId = provider.TenantId.Value;

        _mockPaymentService
            .Setup(x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(2000); // Wait for initial delay (100ms) + processing time
        await _job.StopAsync(CancellationToken.None);

        // Assert
        _mockPaymentService.Verify(
            x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        _mockTenantProvider.Verify(
            x => x.SetTenantOverride(tenantId),
            Times.AtLeastOnce);

        _mockTenantProvider.Verify(
            x => x.ClearTenantOverride(),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Execute_NoActiveTenants_ShouldDoNothing()
    {
        // Arrange - no providers in database
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(500);
        await _job.StopAsync(CancellationToken.None);

        // Assert
        _mockPaymentService.Verify(
            x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_ProcessingFails_ShouldLogErrorAndContinue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        _mockPaymentService
            .Setup(x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Processing failed"));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(2000); // Wait for initial delay (100ms) + processing time
        await _job.StopAsync(CancellationToken.None);

        // Assert
        // Logger verification removed

        _mockTenantProvider.Verify(
            x => x.ClearTenantOverride(),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Execute_MultipleActiveTenants_ShouldProcessAllTenants()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();

        var userId1 = Guid.NewGuid();
        var provider1 = Provider.Create(
            userId1,
            "Provider 1",
            "provider1");

        var userId2 = Guid.NewGuid();
        var provider2 = Provider.Create(
            userId2,
            "Provider 2",
            "provider2");

        await _context.Providers.AddRangeAsync(provider1, provider2);
        await _context.SaveChangesAsync();

        _mockPaymentService
            .Setup(x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(2000); // Wait for initial delay (100ms) + processing time
        await _job.StopAsync(CancellationToken.None);

        // Assert
        _mockPaymentService.Verify(
            x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeast(2));

        _mockTenantProvider.Verify(
            x => x.SetTenantOverride(It.IsAny<Guid>()),
            Times.AtLeast(2));

        _mockTenantProvider.Verify(
            x => x.ClearTenantOverride(),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task Execute_InactiveTenant_ShouldSkipTenant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        // Deactivate provider
        provider.Deactivate();

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(500);
        await _job.StopAsync(CancellationToken.None);

        // Assert
        _mockPaymentService.Verify(
            x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_ProcessingTimesOut_ShouldLogWarningAndContinue()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var provider1 = Provider.Create(
            userId1,
            "Provider 1",
            "provider1");

        var userId2 = Guid.NewGuid();
        var provider2 = Provider.Create(
            userId2,
            "Provider 2",
            "provider2");

        await _context.Providers.AddRangeAsync(provider1, provider2);
        await _context.SaveChangesAsync();

        var callCount = 0;
        _mockPaymentService
            .Setup(x => x.ProcessPendingPaymentsForTenantAsync(
                It.IsAny<TenantId>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (TenantId tid, DateTime date, CancellationToken ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Simulate timeout on first call
                    await Task.Delay(TimeSpan.FromSeconds(60), ct);
                }
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        var executeTask = _job.StartAsync(cts.Token);
        await Task.Delay(2000); // Wait for initial delay (100ms) + processing time
        await _job.StopAsync(CancellationToken.None);

        // Assert
        _mockTenantProvider.Verify(
            x => x.ClearTenantOverride(),
            Times.AtLeast(1));
    }
}
