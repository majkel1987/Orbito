using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Services;
using Orbito.API.Middleware;
using Orbito.Domain.ValueObjects;
using System.Security.Claims;
using Xunit;

namespace Orbito.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class TenantIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<ILogger<TenantMiddleware>> _middlewareLoggerMock;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly TenantContext _tenantContext;

        public TenantIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Mock services
            _middlewareLoggerMock = new Mock<ILogger<TenantMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
            _tenantContext = new TenantContext();

            // Register services
            services.AddSingleton(_middlewareLoggerMock.Object);
            services.AddSingleton<ITenantContext>(_tenantContext);

            _serviceProvider = services.BuildServiceProvider();
        }

        #region TenantId Value Object Tests

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_Create_WithValidGuid_ShouldCreateTenantId()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var tenantId = TenantId.Create(guid);

            // Assert
            tenantId.Should().NotBeNull();
            tenantId.Value.Should().Be(guid);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_Create_WithEmptyGuid_ShouldThrowException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => TenantId.Create(emptyGuid));
            exception.Message.Should().Contain("TenantId cannot be empty");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_New_ShouldCreateNewTenantId()
        {
            // Act
            var tenantId1 = TenantId.New();
            var tenantId2 = TenantId.New();

            // Assert
            tenantId1.Should().NotBeNull();
            tenantId2.Should().NotBeNull();
            tenantId1.Value.Should().NotBe(tenantId2.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_ImplicitConversion_ShouldConvertToGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tenantId = TenantId.Create(guid);

            // Act
            Guid convertedGuid = tenantId;

            // Assert
            convertedGuid.Should().Be(guid);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_ExplicitConversion_ShouldConvertFromGuid()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var tenantId = (TenantId)guid;

            // Assert
            tenantId.Should().NotBeNull();
            tenantId.Value.Should().Be(guid);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_Equals_WithSameValue_ShouldReturnTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tenantId1 = TenantId.Create(guid);
            var tenantId2 = TenantId.Create(guid);

            // Act & Assert
            tenantId1.Equals(tenantId2).Should().BeTrue();
            tenantId1.Should().Be(tenantId2);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_Equals_WithDifferentValue_ShouldReturnFalse()
        {
            // Arrange
            var tenantId1 = TenantId.New();
            var tenantId2 = TenantId.New();

            // Act & Assert
            tenantId1.Equals(tenantId2).Should().BeFalse();
            tenantId1.Should().NotBe(tenantId2);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_GetHashCode_WithSameValue_ShouldReturnSameHashCode()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tenantId1 = TenantId.Create(guid);
            var tenantId2 = TenantId.Create(guid);

            // Act & Assert
            tenantId1.GetHashCode().Should().Be(tenantId2.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_ToString_ShouldReturnGuidString()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tenantId = TenantId.Create(guid);

            // Act
            var result = tenantId.ToString();

            // Assert
            result.Should().Be(guid.ToString());
        }

        #endregion

        #region TenantContext Service Tests

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_InitialState_ShouldHaveNoTenant()
        {
            // Arrange
            var tenantContext = new TenantContext();

            // Act & Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_SetTenant_ShouldSetCurrentTenant()
        {
            // Arrange
            var tenantContext = new TenantContext();
            var tenantId = TenantId.New();

            // Act
            tenantContext.SetTenant(tenantId);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_SetTenant_WithNull_ShouldClearTenant()
        {
            // Arrange
            var tenantContext = new TenantContext();
            var tenantId = TenantId.New();
            tenantContext.SetTenant(tenantId);

            // Act
            tenantContext.SetTenant(null);

            // Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_ClearTenant_ShouldClearCurrentTenant()
        {
            // Arrange
            var tenantContext = new TenantContext();
            var tenantId = TenantId.New();
            tenantContext.SetTenant(tenantId);

            // Act
            tenantContext.ClearTenant();

            // Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_UpdateTenant_ShouldUpdateCurrentTenant()
        {
            // Arrange
            var tenantContext = new TenantContext();
            var tenantId1 = TenantId.New();
            var tenantId2 = TenantId.New();
            tenantContext.SetTenant(tenantId1);

            // Act
            tenantContext.SetTenant(tenantId2);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId2);
        }

        #endregion

        #region TenantMiddleware Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithJWTClaims_ShouldSetTenantContext()
        {
            // Arrange
            var tenantId = TenantId.New();
            var userId = Guid.NewGuid().ToString();
            
            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("tenant_id", tenantId.Value.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithXTenantIdHeader_ShouldSetTenantContext()
        {
            // Arrange
            var tenantId = TenantId.New();
            
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-Id"] = tenantId.Value.ToString();

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithQueryParameter_ShouldSetTenantContext()
        {
            // Arrange
            var tenantId = TenantId.New();
            
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString($"?tenantId={tenantId.Value}");

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithInvalidTenantId_ShouldNotSetTenantContext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-Id"] = "invalid-guid";

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithEmptyTenantId_ShouldNotSetTenantContext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-Id"] = "";

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithMultipleTenantSources_ShouldSetFromLastSource()
        {
            // Arrange
            var jwtTenantId = TenantId.New();
            var headerTenantId = TenantId.New();
            var queryTenantId = TenantId.New();
            var userId = Guid.NewGuid().ToString();
            
            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("tenant_id", jwtTenantId.Value.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);
            context.Request.Headers["X-Tenant-Id"] = headerTenantId.Value.ToString();
            context.Request.QueryString = new QueryString($"?tenantId={queryTenantId.Value}");

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            // Middleware sets tenant from all sources, so the last one (query parameter) will be used
            tenantContext.CurrentTenantId.Should().Be(queryTenantId);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithUnauthenticatedUser_ShouldNotSetTenantFromClaims()
        {
            // Arrange
            var context = new DefaultHttpContext();
            // No authentication setup

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithException_ShouldContinueRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-Id"] = "invalid-guid";

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            // Middleware should not throw exception and continue processing
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_ShouldClearPreviousTenantContext()
        {
            // Arrange
            var previousTenantId = TenantId.New();
            var newTenantId = TenantId.New();
            
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Tenant-Id"] = newTenantId.Value.ToString();

            var tenantContext = new TenantContext();
            tenantContext.SetTenant(previousTenantId); // Set previous tenant

            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(newTenantId);
            tenantContext.CurrentTenantId.Should().NotBe(previousTenantId);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        #endregion

        #region Multi-Tenant Business Logic Tests

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantContext_WithMultipleTenants_ShouldIsolateData()
        {
            // Arrange
            var tenantId1 = TenantId.New();
            var tenantId2 = TenantId.New();
            var tenantContext = new TenantContext();

            // Act & Assert - Tenant 1
            tenantContext.SetTenant(tenantId1);
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId1);

            // Act & Assert - Tenant 2
            tenantContext.SetTenant(tenantId2);
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId2);
            tenantContext.CurrentTenantId.Should().NotBe(tenantId1);

            // Act & Assert - Clear tenant
            tenantContext.ClearTenant();
            tenantContext.HasTenant.Should().BeFalse();
            tenantContext.CurrentTenantId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_WithSameGuid_ShouldBeEqual()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var tenantId1 = TenantId.Create(guid);
            var tenantId2 = TenantId.Create(guid);

            // Act & Assert
            tenantId1.Should().Be(tenantId2);
            tenantId1.GetHashCode().Should().Be(tenantId2.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TenantId_WithDifferentGuid_ShouldNotBeEqual()
        {
            // Arrange
            var tenantId1 = TenantId.New();
            var tenantId2 = TenantId.New();

            // Act & Assert
            tenantId1.Should().NotBe(tenantId2);
            tenantId1.GetHashCode().Should().NotBe(tenantId2.GetHashCode());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TenantMiddleware_WithComplexScenario_ShouldHandleCorrectly()
        {
            // Arrange
            var tenantId = TenantId.New();
            var userId = Guid.NewGuid().ToString();
            
            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("tenant_id", tenantId.Value.ToString()),
                new(ClaimTypes.Email, "test@example.com"),
                new(ClaimTypes.Role, "Provider")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);

            var tenantContext = new TenantContext();
            var middleware = new TenantMiddleware(_nextMock.Object, _middlewareLoggerMock.Object);

            // Act
            await middleware.InvokeAsync(context, tenantContext);

            // Assert
            tenantContext.HasTenant.Should().BeTrue();
            tenantContext.CurrentTenantId.Should().Be(tenantId);
            context.User.Identity!.IsAuthenticated.Should().BeTrue();
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            context.User.FindFirst("tenant_id")?.Value.Should().Be(tenantId.Value.ToString());
            _nextMock.Verify(x => x(context), Times.Once);
        }

        #endregion

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
