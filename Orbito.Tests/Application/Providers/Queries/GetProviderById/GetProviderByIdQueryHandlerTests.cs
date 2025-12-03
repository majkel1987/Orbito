using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Queries.GetProviderById;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Queries.GetProviderById;

[Trait("Category", "Unit")]
public class GetProviderByIdQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<ILogger<GetProviderByIdQueryHandler>> _mockLogger;
    private readonly GetProviderByIdQueryHandler _handler;

    public GetProviderByIdQueryHandlerTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<GetProviderByIdQueryHandler>>();

        _handler = new GetProviderByIdQueryHandler(
            _mockProviderRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldReturnProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());
        var userId = Guid.NewGuid();

        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByIdQuery(providerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SubdomainSlug.Should().Be("testprovider");
        result.Value.BusinessName.Should().Be("Test Provider");

        _mockProviderRepository.Verify(
            x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByIdQuery(providerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Provider.NotFound");

        _mockProviderRepository.Verify(
            x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetProviderByIdQuery(providerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}
