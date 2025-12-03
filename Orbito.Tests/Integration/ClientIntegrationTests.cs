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
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;
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
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CreateClientCommandHandler>> _createLoggerMock;
        private readonly Mock<ILogger<UpdateClientCommandHandler>> _updateLoggerMock;
        private readonly Mock<ILogger<DeleteClientCommandHandler>> _deleteLoggerMock;
        private readonly Mock<ILogger<ActivateClientCommandHandler>> _activateLoggerMock;
        private readonly Mock<ILogger<DeactivateClientCommandHandler>> _deactivateLoggerMock;
        private readonly Mock<ILogger<GetClientByIdQueryHandler>> _getByIdLoggerMock;
        private readonly Mock<ILogger<GetClientsByProviderQueryHandler>> _getByProviderLoggerMock;
        private readonly Mock<ILogger<SearchClientsQueryHandler>> _searchLoggerMock;
        private readonly TenantId _tenantId = TenantId.New();

        public ClientIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Mock services
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _providerRepositoryMock = new Mock<IProviderRepository>();
            
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

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            // Setup Provider repository - Provider.Id == TenantId.Client
            var testProvider = Provider.Create(Guid.NewGuid(), "Test Provider", "test-provider");
            testProvider.Id = _tenantId.Value; // Provider.Id must equal TenantId.Client
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(_tenantId.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testProvider);

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
            services.AddSingleton(_providerRepositoryMock.Object);
            services.AddSingleton(_userManagerMock.Object);
            services.AddSingleton(_createLoggerMock.Object);
            services.AddSingleton(_updateLoggerMock.Object);
            services.AddSingleton(_deleteLoggerMock.Object);
            services.AddSingleton(_activateLoggerMock.Object);
            services.AddSingleton(_deactivateLoggerMock.Object);
            services.AddSingleton(_getByIdLoggerMock.Object);
            services.AddSingleton(_getByProviderLoggerMock.Object);
            services.AddSingleton(_searchLoggerMock.Object);

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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.UserId.Should().Be(userId);
            result.Value.CompanyName.Should().Be(companyName);
            result.Value.Phone.Should().Be(phone);
            result.Value.IsActive.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            // Note: AddAsync calls SaveChangesAsync internally, so we don't verify UnitOfWork.SaveChangesAsync
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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.DirectEmail.Should().Be(directEmail);
            result.Value.DirectFirstName.Should().Be(directFirstName);
            result.Value.DirectLastName.Should().Be(directLastName);
            result.Value.CompanyName.Should().Be(companyName);
            result.Value.Phone.Should().Be(phone);
            result.Value.IsActive.Should().BeTrue();

            _clientRepositoryMock.Verify(x => x.GetByEmailAsync(directEmail, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            // Note: AddAsync calls SaveChangesAsync internally, so we don't verify UnitOfWork.SaveChangesAsync
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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Client.UserAlreadyExists");
            result.Value.Should().BeNull();

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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Client.EmailAlreadyExists");
            result.Value.Should().BeNull();

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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
            result.Value.Should().BeNull();

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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(clientId);
            result.Value.CompanyName.Should().Be(companyName);
            result.Value.Phone.Should().Be(phone);
            result.Value.DirectEmail.Should().Be(directEmail);
            result.Value.DirectFirstName.Should().Be(directFirstName);
            result.Value.DirectLastName.Should().Be(directLastName);

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
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.NotFound");

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(clientId);
            result.Value.DirectEmail.Should().Be("test@example.com");
            result.Value.DirectFirstName.Should().Be("Test");
            result.Value.DirectLastName.Should().Be("User");
            result.Value.CompanyName.Should().Be("Test Company");

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
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.NotFound");

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
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Tenant.CrossTenantAccess");

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(clientId);
            result.Value.IsActive.Should().BeTrue();

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
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(clientId);
            result.Value.IsActive.Should().BeFalse();

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
            result.IsSuccess.Should().BeTrue();

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
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Client.CannotDeleteWithActiveSubscriptions");

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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var createResult = await createHandler.Handle(createCommand, CancellationToken.None);

            // Assert
            createResult.Should().NotBeNull();
            createResult.IsSuccess.Should().BeTrue();
            createResult.Value.Should().NotBeNull();
            createResult.Value!.DirectEmail.Should().Be(directEmail);
            createResult.Value.CompanyName.Should().Be(companyName);
            createResult.Value.Phone.Should().Be(phone);

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
            updateResult.IsSuccess.Should().BeTrue();
            updateResult.Value.Should().NotBeNull();
            updateResult.Value.Id.Should().Be(createdClient.Id);
            updateResult.Value.CompanyName.Should().Be("Updated Complex Company");
            updateResult.Value.Phone.Should().Be("+48987654321");
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
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(default);
            result.Value.Should().BeNull();

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
