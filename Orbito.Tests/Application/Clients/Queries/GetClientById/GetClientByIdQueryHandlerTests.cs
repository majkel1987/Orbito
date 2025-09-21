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
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(_clientId);
            result.Client.TenantId.Should().Be(_tenantId.Value);
        }

        [Fact]
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
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(_clientId);
            result.Client.DirectEmail.Should().Be("test@example.com");
            result.Client.DirectFirstName.Should().Be("John");
            result.Client.DirectLastName.Should().Be("Doe");
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnNotFound()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var query = new GetClientByIdQuery(_clientId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Tenant context is required");
        }

        [Fact]
        public async Task Handle_WithNonExistentClient_ShouldReturnNotFound()
        {
            // Arrange
            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client not found");
        }

        [Fact]
        public async Task Handle_WithDifferentTenant_ShouldReturnNotFound()
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
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Access denied");
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnNotFound()
        {
            // Arrange
            var query = new GetClientByIdQuery(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred while retrieving client");
        }
    }
}
