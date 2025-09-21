using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Commands.UpdateClient;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly UpdateClientCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public UpdateClientCommandHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new UpdateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidData_ShouldUpdateClient()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Old Company");
            client.Id = _clientId;
            client.Phone = "Old Phone";

            var command = new UpdateClientCommand(_clientId, "New Company", "New Phone", null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();

            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new UpdateClientCommand(_clientId, "New Company", null, null, null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Tenant context is required");
        }

        [Fact]
        public async Task Handle_WithNonExistentClient_ShouldReturnFailure()
        {
            // Arrange
            var command = new UpdateClientCommand(_clientId, "New Company", null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client not found");
        }

        [Fact]
        public async Task Handle_WithDifferentTenant_ShouldReturnFailure()
        {
            // Arrange
            var differentTenantId = TenantId.New();
            var client = Client.CreateWithUser(differentTenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new UpdateClientCommand(_clientId, "New Company", null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Access denied");
        }

        [Fact]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "old@example.com", "Old", "User", "Test Company");
            client.Id = _clientId;

            var existingClient = Client.CreateDirect(_tenantId, "existing@example.com", "Existing", "User", "Existing Company");
            existingClient.Id = Guid.NewGuid(); // Different ID

            var command = new UpdateClientCommand(_clientId, null, null, "existing@example.com", null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Email is already used by another client");
        }

        [Fact]
        public async Task Handle_WithSameEmail_ShouldUpdateSuccessfully()
        {
            // Arrange
            var email = "same@example.com";
            var client = Client.CreateDirect(_tenantId, email, "Old", "User", "Test Company");
            client.Id = _clientId;

            var command = new UpdateClientCommand(_clientId, "New Company", null, email, "New", "User");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithDirectClient_ShouldUpdateDirectInfo()
        {
            // Arrange
            var client = Client.CreateDirect(_tenantId, "old@example.com", "Old", "User", "Old Company");
            client.Id = _clientId;

            var command = new UpdateClientCommand(_clientId, "New Company", "New Phone", "new@example.com", "New", "User");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithClientWithUser_ShouldNotUpdateDirectInfo()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;
            client.DirectEmail = "old@example.com";
            client.DirectFirstName = "Old";
            client.DirectLastName = "User";

            var command = new UpdateClientCommand(_clientId, "New Company", null, "new@example.com", "New", "User");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            // Direct info should not be updated for clients with User
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new UpdateClientCommand(_clientId, "New Company", null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred while updating client");
        }
    }
}
