using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models.PaymentGateway;
using Orbito.Domain.ValueObjects;

namespace Orbito.Tests.Helpers;

/// <summary>
/// Base test fixture providing common setup for all payment-related tests
/// </summary>
public abstract class BaseTestFixture
{
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock;
    protected readonly Mock<ITenantContext> TenantContextMock;
    protected readonly Mock<ILogger> LoggerMock;
    protected readonly TenantId TestTenantId;
    protected readonly Guid TestClientId;
    protected readonly Guid TestSubscriptionId;
    protected readonly Guid TestProviderId;
    protected readonly Guid TestPlanId;

    protected BaseTestFixture()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        TenantContextMock = new Mock<ITenantContext>();
        LoggerMock = new Mock<ILogger>();

        // Setup reusable test data
        TestTenantId = TenantId.New();
        TestClientId = Guid.NewGuid();
        TestSubscriptionId = Guid.NewGuid();
        TestProviderId = Guid.NewGuid();
        TestPlanId = Guid.NewGuid();

        // Setup default tenant context
        TenantContextMock.Setup(x => x.HasTenant).Returns(true);
        TenantContextMock.Setup(x => x.CurrentTenantId).Returns(TestTenantId);
    }

    /// <summary>
    /// Creates a mock logger for a specific type
    /// </summary>
    protected Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Sets up tenant context to return no tenant (for negative test cases)
    /// </summary>
    protected void SetupNoTenantContext()
    {
        TenantContextMock.Setup(x => x.HasTenant).Returns(false);
        TenantContextMock.Setup(x => x.CurrentTenantId).Returns((TenantId?)null);
    }

    /// <summary>
    /// Sets up tenant context with a different tenant (for tenant mismatch tests)
    /// </summary>
    protected void SetupDifferentTenantContext()
    {
        var differentTenantId = TenantId.New();
        TenantContextMock.Setup(x => x.HasTenant).Returns(true);
        TenantContextMock.Setup(x => x.CurrentTenantId).Returns(differentTenantId);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync was called exactly once
    /// </summary>
    protected void VerifySaveChangesCalledOnce()
    {
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync was never called
    /// </summary>
    protected void VerifySaveChangesNeverCalled()
    {
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
