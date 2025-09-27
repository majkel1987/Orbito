using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Clients.Commands.CreateClient;
using Orbito.Application.Clients.Commands.UpdateClient;
using Orbito.Application.Clients.Commands.DeleteClient;
using Orbito.Application.Clients.Commands.ActivateClient;
using Orbito.Application.Clients.Commands.DeactivateClient;
using Orbito.Application.Clients.Queries.GetClientById;
using Orbito.Application.Clients.Queries.GetClientsByProvider;
using Orbito.Application.Clients.Queries.SearchClients;
using Orbito.Application.Clients.Queries.GetClientStats;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Orbito.Domain.Identity;
using Xunit;

namespace Orbito.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ClientIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CreateClientCommandHandler>> _createLoggerMock;
        private readonly Mock<ILogger<UpdateClientCommandHandler>> _updateLoggerMock;
        private readonly Mock<ILogger<DeleteClientCommandHandler>> _deleteLoggerMock;
        private readonly Mock<ILogger<ActivateClientCommandHandler>> _activateLoggerMock;
        private readonly Mock<ILogger<DeactivateClientCommandHandler>> _deactivateLoggerMock;
        private readonly Mock<ILogger<GetClientByIdQueryHandler>> _getByIdLoggerMock;
        private readonly Mock<ILogger<GetClientsByProviderQueryHandler>> _getByProviderLoggerMock;
        private readonly Mock<ILogger<SearchClientsQueryHandler>> _searchLoggerMock;
        private readonly Mock<ILogger<GetClientStatsQueryHandler>> _statsLoggerMock;
        private readonly TenantId _tenantId = TenantId.New();

        public ClientIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Mock services
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            
            // Fix nullable reference warnings by using null! or proper mock setup
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null!, null!, null!, null!, null!, null!, null!, null!);
            
            _createLoggerMock = new Mock<ILogger<CreateClientCommandHandler>>();
            _updateLoggerMock = new Mock<ILogger<UpdateClientCommandHandler>>();
            _deleteLoggerMock = new Mock<ILogger<DeleteClientCommandHandler>>();
            _activateLoggerMock = new Mock<ILogger<ActivateClientCommandHandler>>();
            _deactivateLoggerMock = new Mock<ILogger<DeactivateClientCommandHandler>>();
            _getByIdLoggerMock = new Mock<ILogger<GetClientByIdQueryHandler>>();
            _getByProviderLoggerMock = new Mock<ILogger<GetClientsByProviderQueryHandler>>();
            _searchLoggerMock = new Mock<ILogger<SearchClientsQueryHandler>>();
            _statsLoggerMock = new Mock<ILogger<GetClientStatsQueryHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            // Setup UserManager mock
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new ApplicationUser
                {
                    Id = Guid.Parse(id),
                    Email = "test@example.com",
                    UserName = "test@example.com",
                    FirstName = "Test",
                    LastName = "User"
                });

            // Register services
            services.AddSingleton(_tenantContextMock.Object);
            services.AddSingleton(_unitOfWorkMock.Object);
            services.AddSingleton(_clientRepositoryMock.Object);
            services.AddSingleton(_userManagerMock.Object);
            services.AddSingleton(_createLoggerMock.Object);
            services.AddSingleton(_updateLoggerMock.Object);
            services.AddSingleton(_deleteLoggerMock.Object);
            services.AddSingleton(_activateLoggerMock.Object);
            services.AddSingleton(_deactivateLoggerMock.Object);
            services.AddSingleton(_getByIdLoggerMock.Object);
            services.AddSingleton(_getByProviderLoggerMock.Object);
            services.AddSingleton(_searchLoggerMock.Object);
            services.AddSingleton(_statsLoggerMock.Object);

            _serviceProvider = services.BuildServiceProvider();
        }

        #region Create Client Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithValidUserData_ShouldCreateClientSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var companyName = "Test Company";
            var phone = "+48123456789";

            var command = new CreateClientCommand(userId, companyName, phone, null, null, null);

            var createdClient = Client.CreateWithUser(_tenantId, userId, companyName);
            createdClient.Phone = phone;
            
            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.UserId.Should().Be(userId);
            result.Client.CompanyName.Should().Be(companyName);
            result.Client.Phone.Should().Be(phone);
            result.Client.IsActive.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithValidDirectData_ShouldCreateClientSuccessfully()
        {
            // Arrange
            var directEmail = "client@example.com";
            var directFirstName = "Jan";
            var directLastName = "Kowalski";
            var companyName = "Test Company";
            var phone = "+48123456789";

            var command = new CreateClientCommand(null, companyName, phone, directEmail, directFirstName, directLastName);

            var createdClient = Client.CreateDirect(_tenantId, directEmail, directFirstName, directLastName, companyName);
            createdClient.Phone = phone;
            
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.DirectEmail.Should().Be(directEmail);
            result.Client.DirectFirstName.Should().Be(directFirstName);
            result.Client.DirectLastName.Should().Be(directLastName);
            result.Client.CompanyName.Should().Be(companyName);
            result.Client.Phone.Should().Be(phone);
            result.Client.IsActive.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithExistingUser_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var companyName = "Test Company";

            var command = new CreateClientCommand(userId, companyName, null, null, null, null);

            var existingClient = Client.CreateWithUser(_tenantId, userId, "Existing Company");
            
            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client with this user already exists");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var directEmail = "existing@example.com";
            var directFirstName = "Jan";
            var directLastName = "Kowalski";
            var companyName = "Test Company";

            var command = new CreateClientCommand(null, companyName, null, directEmail, directFirstName, directLastName);

            var existingClient = Client.CreateDirect(_tenantId, directEmail, "Existing", "User", "Existing Company");
            
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client with this email already exists");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var companyName = "Test Company";

            var command = new CreateClientCommand(userId, companyName, null, null, null, null);

            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Tenant context is required");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateClient_WithInvalidData_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateClientCommand(null, "Test Company", null, null, null, null);

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Either UserId or DirectEmail must be provided");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Update Client Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdateClient_WithValidData_ShouldUpdateClientSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var companyName = "Updated Company";
            var phone = "+48987654321";
            var directEmail = "updated@example.com";
            var directFirstName = "Updated";
            var directLastName = "Name";

            var command = new UpdateClientCommand(clientId, companyName, phone, directEmail, directFirstName, directLastName);

            var existingClient = Client.CreateDirect(_tenantId, "old@example.com", "Old", "Name", "Old Company");
            existingClient.Id = clientId;

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(clientId);
            result.Client.CompanyName.Should().Be(companyName);
            result.Client.Phone.Should().Be(phone);
            result.Client.DirectEmail.Should().Be(directEmail);
            result.Client.DirectFirstName.Should().Be(directFirstName);
            result.Client.DirectLastName.Should().Be(directLastName);

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdateClient_WithNonExistentClient_ShouldReturnFailure()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var companyName = "Updated Company";

            var command = new UpdateClientCommand(clientId, companyName, null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            var handler = new UpdateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client not found");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Get Client Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetClientById_WithValidId_ShouldReturnClient()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            var expectedClient = Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company");
            expectedClient.Id = clientId;

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedClient);

            var handler = new GetClientByIdQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(clientId);
            result.Client.DirectEmail.Should().Be("test@example.com");
            result.Client.DirectFirstName.Should().Be("Test");
            result.Client.DirectLastName.Should().Be("User");
            result.Client.CompanyName.Should().Be("Test Company");

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetClientById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);

            var handler = new GetClientByIdQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client not found");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetClientById_WithDifferentTenant_ShouldReturnAccessDenied()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            var differentTenantId = TenantId.New();
            var clientFromDifferentTenant = Client.CreateDirect(differentTenantId, "test@example.com", "Test", "User", "Test Company");
            clientFromDifferentTenant.Id = clientId;

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientFromDifferentTenant);

            var handler = new GetClientByIdQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Access denied");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        #endregion

        #region Activate/Deactivate Client Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ActivateClient_WithValidClient_ShouldActivateClient()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new ActivateClientCommand(clientId);

            var inactiveClient = Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company");
            inactiveClient.Id = clientId;
            inactiveClient.Deactivate(); // Make it inactive

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new ActivateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(clientId);
            result.Client.IsActive.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeactivateClient_WithValidClient_ShouldDeactivateClient()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new DeactivateClientCommand(clientId);

            var activeClient = Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company");
            activeClient.Id = clientId;
            activeClient.Activate(); // Make it active

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeactivateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Client.Should().NotBeNull();
            result.Client!.Id.Should().Be(clientId);
            result.Client.IsActive.Should().BeFalse();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Delete Client Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteClient_WithValidClient_ShouldDeleteClient()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new DeleteClientCommand(clientId, false); // Soft delete

            var client = Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company");
            client.Id = clientId;

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _clientRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.CanClientBeDeletedAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteClient_WithActiveSubscriptions_ShouldReturnFailure()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var command = new DeleteClientCommand(clientId, false); // Soft delete

            var client = Client.CreateDirect(_tenantId, "test@example.com", "Test", "User", "Test Company");
            client.Id = clientId;

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);
            _clientRepositoryMock.Setup(x => x.CanClientBeDeletedAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Cannot be deleted due to active subscriptions

            var handler = new DeleteClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Client cannot be deleted because it has active subscriptions or payments");

            _clientRepositoryMock.Verify(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _clientRepositoryMock.Verify(x => x.CanClientBeDeletedAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Business Logic Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ClientBusinessLogic_ShouldHandleComplexScenarios()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var directEmail = "complex@example.com";
            var directFirstName = "Complex";
            var directLastName = "Client";
            var companyName = "Complex Company";
            var phone = "+48123456789";

            // Test 1: Create Direct Client
            var createCommand = new CreateClientCommand(null, companyName, phone, directEmail, directFirstName, directLastName);
            var createdClient = Client.CreateDirect(_tenantId, directEmail, directFirstName, directLastName, companyName);
            createdClient.Phone = phone;
            
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            var createHandler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var createResult = await createHandler.Handle(createCommand, CancellationToken.None);

            // Assert
            createResult.Should().NotBeNull();
            createResult.Success.Should().BeTrue();
            createResult.Client.Should().NotBeNull();
            createResult.Client!.DirectEmail.Should().Be(directEmail);
            createResult.Client.CompanyName.Should().Be(companyName);
            createResult.Client.Phone.Should().Be(phone);

            // Test 2: Update Client
            var updateCommand = new UpdateClientCommand(
                createdClient.Id,
                "Updated Complex Company",
                "+48987654321",
                "updated@example.com",
                "Updated",
                "Name");

            _clientRepositoryMock.Setup(x => x.GetByIdAsync(createdClient.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var updateHandler = new UpdateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var updateResult = await updateHandler.Handle(updateCommand, CancellationToken.None);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.Success.Should().BeTrue();
            updateResult.Client.Should().NotBeNull();
            updateResult.Client!.Id.Should().Be(createdClient.Id);
            updateResult.Client.CompanyName.Should().Be("Updated Complex Company");
            updateResult.Client.Phone.Should().Be("+48987654321");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ClientValidation_ShouldHandleEdgeCases()
        {
            // Arrange
            var directEmail = "edge@example.com";
            var directFirstName = "Edge";
            var directLastName = "Case";
            var companyName = "Edge Case Company";

            var command = new CreateClientCommand(null, companyName, null, directEmail, directFirstName, directLastName);

            // Simulate database error
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred while creating client");
            result.Client.Should().BeNull();

            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
