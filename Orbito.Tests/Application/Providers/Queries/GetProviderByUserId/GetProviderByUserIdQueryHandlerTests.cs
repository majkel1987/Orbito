using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Queries.GetProviderByUserId;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Queries.GetProviderByUserId;

[Trait("Category", "Unit")]
public class GetProviderByUserIdQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<ILogger<GetProviderByUserIdQueryHandler>> _mockLogger;
    private readonly GetProviderByUserIdQueryHandler _handler;

    public GetProviderByUserIdQueryHandlerTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<GetProviderByUserIdQueryHandler>>();

        _handler = new GetProviderByUserIdQueryHandler(
            _mockProviderRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        _mockProviderRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SubdomainSlug.Should().Be("testprovider");
        result.Value.BusinessName.Should().Be("Test Provider");

        _mockProviderRepository.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Provider.NotFound);

        _mockProviderRepository.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
