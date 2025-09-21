using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Services;
using Xunit;

namespace Orbito.Tests.Application.Common.Services
{
    public class ProviderServiceTests
    {
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<ILogger<ProviderService>> _loggerMock;
        private readonly ProviderService _providerService;

        public ProviderServiceTests()
        {
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _loggerMock = new Mock<ILogger<ProviderService>>();

            _providerService = new ProviderService(
                _providerRepositoryMock.Object,
                _loggerMock.Object);
        }

        #region ValidateSubdomainAsync Tests

        [Fact]
        public async Task ValidateSubdomainAsync_WithValidAvailableSubdomain_ShouldReturnTrue()
        {
            // Arrange
            var subdomainSlug = "valid-subdomain";
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync(subdomainSlug, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithTakenSubdomain_ShouldReturnFalse()
        {
            // Arrange
            var subdomainSlug = "taken-subdomain";
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync(subdomainSlug, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithEmptySubdomain_ShouldReturnFalse()
        {
            // Arrange
            var subdomainSlug = "";

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithNullSubdomain_ShouldReturnFalse()
        {
            // Arrange
            string? subdomainSlug = null;

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithWhitespaceSubdomain_ShouldReturnFalse()
        {
            // Arrange
            var subdomainSlug = "   ";

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithReservedSubdomain_ShouldReturnFalse()
        {
            // Arrange
            var reservedSubdomains = new[] { "admin", "api", "www", "app", "dashboard", "support", "help", "docs" };

            foreach (var subdomain in reservedSubdomains)
            {
                // Act
                var result = await _providerService.ValidateSubdomainAsync(subdomain);

                // Assert
                result.Should().BeFalse($"Reserved subdomain '{subdomain}' should be invalid");
            }
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithReservedSubdomainCaseInsensitive_ShouldReturnFalse()
        {
            // Arrange
            var reservedSubdomains = new[] { "ADMIN", "API", "WWW", "APP", "DASHBOARD", "SUPPORT", "HELP", "DOCS" };

            foreach (var subdomain in reservedSubdomains)
            {
                // Act
                var result = await _providerService.ValidateSubdomainAsync(subdomain);

                // Assert
                result.Should().BeFalse($"Reserved subdomain '{subdomain}' should be invalid (case insensitive)");
            }
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WithExcludeProviderId_ShouldPassExcludeId()
        {
            // Arrange
            var subdomainSlug = "test-subdomain";
            var excludeProviderId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync(subdomainSlug, excludeProviderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug, excludeProviderId);

            // Assert
            result.Should().BeTrue();
            _providerRepositoryMock.Verify(x => x.IsSubdomainAvailableAsync(subdomainSlug, excludeProviderId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateSubdomainAsync_WhenRepositoryThrowsException_ShouldReturnFalse()
        {
            // Arrange
            var subdomainSlug = "test-subdomain";
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync(subdomainSlug, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _providerService.ValidateSubdomainAsync(subdomainSlug);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CanProviderBeDeletedAsync Tests

        [Fact]
        public async Task CanProviderBeDeletedAsync_WithProviderWithoutActiveClients_ShouldReturnTrue()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Provider
                {
                    Id = providerId,
                    ActiveClientsCount = 0
                });

            // Act
            var result = await _providerService.CanProviderBeDeletedAsync(providerId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanProviderBeDeletedAsync_WithProviderWithActiveClients_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Provider
                {
                    Id = providerId,
                    ActiveClientsCount = 5
                });

            // Act
            var result = await _providerService.CanProviderBeDeletedAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanProviderBeDeletedAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Provider?)null);

            // Act
            var result = await _providerService.CanProviderBeDeletedAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanProviderBeDeletedAsync_WhenRepositoryThrowsException_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _providerService.CanProviderBeDeletedAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetProviderWithMetricsAsync Tests

        [Fact]
        public async Task GetProviderWithMetricsAsync_WithExistingProvider_ShouldReturnProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var provider = new Domain.Entities.Provider
            {
                Id = providerId,
                BusinessName = "Test Business",
                ActiveClientsCount = 10
            };

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(provider);

            // Act
            var result = await _providerService.GetProviderWithMetricsAsync(providerId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(providerId);
            result.BusinessName.Should().Be("Test Business");
        }

        [Fact]
        public async Task GetProviderWithMetricsAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Provider?)null);

            // Act
            var result = await _providerService.GetProviderWithMetricsAsync(providerId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProviderWithMetricsAsync_WhenRepositoryThrowsException_ShouldReturnNull()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _providerService.GetProviderWithMetricsAsync(providerId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateProviderMetricsAsync Tests

        [Fact]
        public async Task UpdateProviderMetricsAsync_WithExistingProvider_ShouldUpdateMetrics()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var provider = new Domain.Entities.Provider
            {
                Id = providerId,
                BusinessName = "Test Business"
            };

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(provider);
            _providerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Domain.Entities.Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _providerService.UpdateProviderMetricsAsync(providerId);

            // Assert
            _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Provider>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProviderMetricsAsync_WithNonExistentProvider_ShouldNotUpdate()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Provider?)null);

            // Act
            await _providerService.UpdateProviderMetricsAsync(providerId);

            // Assert
            _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Domain.Entities.Provider>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProviderMetricsAsync_WhenRepositoryThrowsException_ShouldNotThrow()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await _providerService.Invoking(x => x.UpdateProviderMetricsAsync(providerId))
                .Should().NotThrowAsync();
        }

        #endregion

        #region IsProviderActiveAsync Tests

        [Fact]
        public async Task IsProviderActiveAsync_WithActiveProvider_ShouldReturnTrue()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var provider = new Domain.Entities.Provider
            {
                Id = providerId,
                IsActive = true
            };

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(provider);

            // Act
            var result = await _providerService.IsProviderActiveAsync(providerId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsProviderActiveAsync_WithInactiveProvider_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var provider = new Domain.Entities.Provider
            {
                Id = providerId,
                IsActive = false
            };

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(provider);

            // Act
            var result = await _providerService.IsProviderActiveAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsProviderActiveAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Provider?)null);

            // Act
            var result = await _providerService.IsProviderActiveAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsProviderActiveAsync_WhenRepositoryThrowsException_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _providerService.IsProviderActiveAsync(providerId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
