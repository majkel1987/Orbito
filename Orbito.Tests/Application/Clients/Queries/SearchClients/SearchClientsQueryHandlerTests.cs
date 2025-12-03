using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Queries.SearchClients;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Queries.SearchClients
{
    public class SearchClientsQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly SearchClientsQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public SearchClientsQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new SearchClientsQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithValidSearchTerm_ShouldReturnSearchResults()
        {
            // Arrange
            var searchTerm = "test";
            var searchResults = new List<Client>
            {
                Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company"),
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company Inc")
            };

            var query = new SearchClientsQuery(searchTerm, 1, 10, false);

            _clientRepositoryMock.Setup(x => x.SearchClientsAsync(searchTerm, 1, 10, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResults);
            _clientRepositoryMock.Setup(x => x.GetSearchCountAsync(searchTerm, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(2);
            result.Value.PageNumber.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
            result.Value.TotalPages.Should().Be(1);

            _clientRepositoryMock.Verify(x => x.SearchClientsAsync(searchTerm, 1, 10, false, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.GetSearchCountAsync(searchTerm, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithActiveOnlyTrue_ShouldSearchOnlyActiveClients()
        {
            // Arrange
            var searchTerm = "active";
            var activeResults = new List<Client>
            {
                Client.CreateDirect(_tenantId, "active@example.com", "Active", "User", "Active Company")
            };

            var query = new SearchClientsQuery(searchTerm, 1, 10, true);

            _clientRepositoryMock.Setup(x => x.SearchClientsAsync(searchTerm, 1, 10, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeResults);
            _clientRepositoryMock.Setup(x => x.GetSearchCountAsync(searchTerm, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Items.Should().HaveCount(1);

            _clientRepositoryMock.Verify(x => x.SearchClientsAsync(searchTerm, 1, 10, true, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.GetSearchCountAsync(searchTerm, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithPagination_ShouldCalculateTotalPages()
        {
            // Arrange
            var searchTerm = "company";
            var searchResults = new List<Client>
            {
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 1"),
                Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Company 2")
            };

            var query = new SearchClientsQuery(searchTerm, 2, 5, false);

            _clientRepositoryMock.Setup(x => x.SearchClientsAsync(searchTerm, 2, 5, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResults);
            _clientRepositoryMock.Setup(x => x.GetSearchCountAsync(searchTerm, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(12); // Total 12 results, page size 5, so 3 pages

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Items.Should().HaveCount(2);
            result.Value.TotalCount.Should().Be(12);
            result.Value.PageNumber.Should().Be(2);
            result.Value.PageSize.Should().Be(5);
            result.Value.TotalPages.Should().Be(3); // Math.Ceiling(12/5) = 3
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptySearchTerm_ShouldReturnFailure()
        {
            // Arrange
            var query = new SearchClientsQuery("", 1, 10, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Validation.Required");
            result.Error.Message.Should().Contain("SearchTerm");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNullSearchTerm_ShouldReturnFailure()
        {
            // Arrange
            var query = new SearchClientsQuery(null!, 1, 10, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Validation.Required");
            result.Error.Message.Should().Contain("SearchTerm");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithWhitespaceSearchTerm_ShouldReturnFailure()
        {
            // Arrange
            var query = new SearchClientsQuery("   ", 1, 10, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Validation.Required");
            result.Error.Message.Should().Contain("SearchTerm");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var query = new SearchClientsQuery("test", 1, 10, false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var searchTerm = "test";
            var query = new SearchClientsQuery(searchTerm, 1, 10, false);

            _clientRepositoryMock.Setup(x => x.SearchClientsAsync(searchTerm, 1, 10, false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNoSearchResults_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "nonexistent";
            var query = new SearchClientsQuery(searchTerm, 1, 10, false);

            _clientRepositoryMock.Setup(x => x.SearchClientsAsync(searchTerm, 1, 10, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Client>());
            _clientRepositoryMock.Setup(x => x.GetSearchCountAsync(searchTerm, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            result.Value.TotalPages.Should().Be(0);
        }
    }
}
