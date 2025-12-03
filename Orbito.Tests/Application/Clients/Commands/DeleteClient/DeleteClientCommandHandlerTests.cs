using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Commands.DeleteClient;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.DeleteClient
{
    public class DeleteClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeleteClientCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();
        private readonly Guid _clientId = Guid.NewGuid();

        public DeleteClientCommandHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new DeleteClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithSoftDelete_ShouldDeactivateClient()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new DeleteClientCommand(_clientId, false);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _clientRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithHardDelete_ShouldDeleteClient()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new DeleteClientCommand(_clientId, true);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _clientRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new DeleteClientCommand(_clientId, false);

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
            var command = new DeleteClientCommand(_clientId, false);

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

            var command = new DeleteClientCommand(_clientId, false);

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
        public async Task Handle_WhenClientCannotBeDeleted_ShouldReturnFailure()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new DeleteClientCommand(_clientId, false);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.CannotDeleteWithActiveSubscriptions");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var client = Client.CreateWithUser(_tenantId, Guid.NewGuid(), "Test Company");
            client.Id = _clientId;

            var command = new DeleteClientCommand(_clientId, false);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(_clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(_clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
