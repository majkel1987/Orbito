using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Queries.GetClientsByProvider;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Queries.GetClientsByProvider
{
    public class GetClientsByProviderQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetClientsByProviderQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetClientsByProviderQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            // Default: valid tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new GetClientsByProviderQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);
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

            var query = new GetClientsByProviderQuery(1, 10, null, null);

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

            var query = new GetClientsByProviderQuery(1, 10, "active", null);

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

            var query = new GetClientsByProviderQuery(1, 10, null, searchTerm);

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

            var query = new GetClientsByProviderQuery(2, 5, null, null);

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

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenNoTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            var query = new GetClientsByProviderQuery(1, 10, null, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Tenant.NoTenantContext);

            // Verify repository was never called
            _clientRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = new GetClientsByProviderQuery(1, 10, null, null);

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
            var query = new GetClientsByProviderQuery(1, 10, null, null);

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
