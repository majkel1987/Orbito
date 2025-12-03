using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Commands.DeactivateClient;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.DeactivateClient
{
    public class DeactivateClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeactivateClientCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public DeactivateClientCommandHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new DeactivateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithActiveClient_ShouldDeactivateClient()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;
            // Client is active by default

            var inactiveClient = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            inactiveClient.Id = _clientId;
            inactiveClient.Deactivate();

            var command = new DeactivateClientCommand(_clientId);

            _clientRepositoryMock.SetupSequence(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client)
                .ReturnsAsync(inactiveClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.IsActive.Should().BeFalse();

            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new DeactivateClientCommand(_clientId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

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
            var command = new DeactivateClientCommand(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

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

            var command = new DeactivateClientCommand(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.CrossTenantAccess");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithAlreadyInactiveClient_ShouldReturnFailure()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;
            client.Deactivate(); // Make it inactive

            var command = new DeactivateClientCommand(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.AlreadyInactive");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new DeactivateClientCommand(_clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
