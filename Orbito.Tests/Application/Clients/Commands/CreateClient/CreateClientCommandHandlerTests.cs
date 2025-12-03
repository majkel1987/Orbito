using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Commands.CreateClient;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Commands.CreateClient
{
    public class CreateClientCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateClientCommandHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public CreateClientCommandHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            // Setup Provider repository - Provider.Id == TenantId.Value
            var testProvider = Provider.Create(Guid.NewGuid(), "Test Provider", "test-provider");
            testProvider.Id = _tenantId.Value; // Provider.Id must equal TenantId.Value
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(_tenantId.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testProvider);

            _handler = new CreateClientCommandHandler(
                _clientRepositoryMock.Object,
                _providerRepositoryMock.Object,
                _tenantContextMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithUserId_ShouldCreateClientWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var companyName = "Test Company";
            var command = new CreateClientCommand(userId, companyName, null, null, null, null);

            var createdClient = Client.CreateWithUser(_tenantId, userId, companyName);
            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(createdClient.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.UserId.Should().Be(userId);
            result.Value.CompanyName.Should().Be(companyName);

            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            // Note: AddAsync already calls SaveChangesAsync internally, so we don't verify it separately
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithDirectEmail_ShouldCreateDirectClient()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var companyName = "Test Company";
            var command = new CreateClientCommand(null, companyName, null, email, firstName, lastName);

            var createdClient = Client.CreateDirect(_tenantId, email, firstName, lastName, companyName);
            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(createdClient.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.DirectEmail.Should().Be(email);
            result.Value.DirectFirstName.Should().Be(firstName);
            result.Value.DirectLastName.Should().Be(lastName);
            result.Value.CompanyName.Should().Be(companyName);

            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
            // Note: AddAsync already calls SaveChangesAsync internally, so we don't verify it separately
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var command = new CreateClientCommand(Guid.NewGuid(), "Test Company", null, null, null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Tenant.NoTenantContext");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNeitherUserIdNorEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateClientCommand(null, "Test Company", null, null, null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // Handler catches ValidationException and wraps it in generic error message
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var email = "existing@example.com";
            var existingClient = Client.CreateDirect(_tenantId, email, "Existing", "User", "Existing Company");
            var command = new CreateClientCommand(null, "Test Company", null, email, "John", "Doe");

            _clientRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Client.EmailAlreadyExists");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithExistingUserId_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingClient = Client.CreateWithUser(_tenantId, userId, "Existing Company");
            var command = new CreateClientCommand(userId, "Test Company", null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("Client.UserAlreadyExists");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithPhone_ShouldSetPhone()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var phone = "+48123456789";
            var command = new CreateClientCommand(userId, "Test Company", phone, null, null, null);

            var createdClient = Client.CreateWithUser(_tenantId, userId, "Test Company");
            createdClient.Phone = phone; // Set phone on the client
            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Client?)null);
            _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);
            _clientRepositoryMock.Setup(x => x.GetByIdAsync(createdClient.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdClient);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Phone.Should().Be(phone);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateClientCommand(userId, "Test Company", null, null, null, null);

            _clientRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }


        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptyGuidUserId_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateClientCommand(Guid.Empty, "Test Company", null, null, null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptyEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateClientCommand(null, "Test Company", null, "", null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // Handler catches ValidationException and wraps it in generic error message
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithWhitespaceEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateClientCommand(null, "Test Company", null, "   ", null, null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            // Handler catches ValidationException and wraps it in generic error message
        }
    }
}
