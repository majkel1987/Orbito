using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Queries.GetClientsByProvider;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Queries.GetClientsByProvider
{
    public class GetClientsByProviderQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly GetClientsByProviderQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetClientsByProviderQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();

            _handler = new GetClientsByProviderQueryHandler(
                _clientRepositoryMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithActiveOnlyFalse_ShouldReturnAllClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 1"),
                Client.CreateDirect(_tenantId, "test1@example.com", "John", "Doe", "Company 2"),
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 3")
            };

            var query = new GetClientsByProviderQuery(1, 10, false, null);

            _clientRepositoryMock.Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clients);
            _clientRepositoryMock.Setup(x => x.GetTotalCountAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(3);
            result.Value.TotalCount.Should().Be(3);
            result.Value.PageNumber.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
            result.Value.TotalPages.Should().Be(1);

            _clientRepositoryMock.Verify(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.GetTotalCountAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithActiveOnlyTrue_ShouldReturnActiveClients()
        {
            // Arrange
            var activeClients = new List<Client>
            {
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Active Company 1"),
                Client.CreateDirect(_tenantId, "active@example.com", "Active", "User", "Active Company 2")
            };

            var query = new GetClientsByProviderQuery(1, 10, true, null);

            _clientRepositoryMock.Setup(x => x.GetActiveClientsAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeClients);
            _clientRepositoryMock.Setup(x => x.GetActiveClientsCountAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(2);

            _clientRepositoryMock.Verify(x => x.GetActiveClientsAsync(1, 10, null, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.GetActiveClientsCountAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithSearchTerm_ShouldReturnFilteredClients()
        {
            // Arrange
            var searchTerm = "test";
            var filteredClients = new List<Client>
            {
                Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company")
            };

            var query = new GetClientsByProviderQuery(1, 10, false, searchTerm);

            _clientRepositoryMock.Setup(x => x.GetAllAsync(1, 10, searchTerm, It.IsAny<CancellationToken>()))
                .ReturnsAsync(filteredClients);
            _clientRepositoryMock.Setup(x => x.GetTotalCountAsync(searchTerm, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(1);
            result.Value.TotalCount.Should().Be(1);

            _clientRepositoryMock.Verify(x => x.GetAllAsync(1, 10, searchTerm, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.GetTotalCountAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithPagination_ShouldCalculateTotalPages()
        {
            // Arrange
            var clients = new List<Client>
            {
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 1"),
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 2")
            };

            var query = new GetClientsByProviderQuery(2, 5, false, null);

            _clientRepositoryMock.Setup(x => x.GetAllAsync(2, 5, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clients);
            _clientRepositoryMock.Setup(x => x.GetTotalCountAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(12); // Total 12 clients, page size 5, so 3 pages

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(12);
            result.Value.PageNumber.Should().Be(2);
            result.Value.PageSize.Should().Be(5);
            result.Value.TotalPages.Should().Be(3); // Math.Ceiling(12/5) = 3
        }

        // Note: Tenant context validation is now handled by query filters in ApplicationDbContext
        // Query filters automatically return empty results if no tenant context is available

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = new GetClientsByProviderQuery(1, 10, false, null);

            _clientRepositoryMock.Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetClientsByProviderQuery(1, 10, false, null);

            _clientRepositoryMock.Setup(x => x.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Client>());
            _clientRepositoryMock.Setup(x => x.GetTotalCountAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            result.Value.TotalPages.Should().Be(0);
        }
    }
}
