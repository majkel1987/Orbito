using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Queries.GetClientById;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Queries.GetClientById
{
    public class GetClientByIdQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetClientByIdQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public GetClientByIdQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new GetClientByIdQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithExistingClient_ShouldReturnClient()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(_clientId);
            result.Value.TenantId.Should().Be(_tenantId.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithDirectClient_ShouldReturnClient()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "test@example.com", "John", "Doe", "Test Company");
            client.Id = _clientId;

            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(_clientId);
            result.Value.DirectEmail.Should().Be("test@example.com");
            result.Value.DirectFirstName.Should().Be("John");
            result.Value.DirectLastName.Should().Be("Doe");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var query = new GetClientByIdQuery(_clientId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNonExistentClient_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.NotFound");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithDifferentTenant_ShouldReturnFailure()
        {
            // Arrange
            var differentTenantId = TenantId.New();
            var client = Client.CreateWithUser(differentTenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.CrossTenantAccess");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
