using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Queries.GetAllProviders;
using Orbito.Domain.Entities;
using Xunit;

namespace Orbito.Tests.Application.Providers.Queries.GetAllProviders;

[Trait("Category", "Unit")]
public class GetAllProvidersQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<ILogger<GetAllProvidersQueryHandler>> _mockLogger;
    private readonly GetAllProvidersQueryHandler _handler;

    public GetAllProvidersQueryHandlerTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<GetAllProvidersQueryHandler>>();

        _handler = new GetAllProvidersQueryHandler(
            _mockProviderRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var providers = new List<Provider>
        {
            Provider.Create(Guid.NewGuid(), "Provider 1", "provider1"),
            Provider.Create(Guid.NewGuid(), "Provider 2", "provider2")
        };

        _mockProviderRepository
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        _mockProviderRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetAllProvidersQuery(PageNumber: 1, PageSize: 10, ActiveOnly: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);

        _mockProviderRepository.Verify(
            x => x.GetAllAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockProviderRepository.Verify(
            x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        _mockProviderRepository
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());

        _mockProviderRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetAllProvidersQuery(PageNumber: 1, PageSize: 10, ActiveOnly: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var providers = new List<Provider>
        {
            Provider.Create(Guid.NewGuid(), "Provider 1", "provider1")
        };

        _mockProviderRepository
            .Setup(x => x.GetAllAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        _mockProviderRepository
            .Setup(x => x.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var query = new GetAllProvidersQuery(PageNumber: 2, PageSize: 1, ActiveOnly: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(10);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(1);
        result.Value.TotalPages.Should().Be(10);

        _mockProviderRepository.Verify(
            x => x.GetAllAsync(2, 1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ActiveOnlyTrue_ShouldReturnOnlyActiveProviders()
    {
        // Arrange
        var activeProviders = new List<Provider>
        {
            Provider.Create(Guid.NewGuid(), "Provider 1", "provider1")
        };

        _mockProviderRepository
            .Setup(x => x.GetActiveProvidersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeProviders);

        _mockProviderRepository
            .Setup(x => x.GetActiveCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetAllProvidersQuery(PageNumber: 1, PageSize: 10, ActiveOnly: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);

        _mockProviderRepository.Verify(
            x => x.GetActiveProvidersAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockProviderRepository.Verify(
            x => x.GetActiveCountAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mockProviderRepository.Verify(
            x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
