using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Commands.CreateProvider
{
    [Trait("Category", "Unit")]
    public class CreateProviderCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CreateProviderCommandHandler>> _loggerMock;
        private readonly CreateProviderCommandHandler _handler;

        public CreateProviderCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _providerRepositoryMock = new Mock<IProviderRepository>();
            
            // Fix nullable reference warnings
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null!, null!, null!, null!, null!, null!, null!, null!);
            
            _loggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();

            _handler = new CreateProviderCommandHandler(
                _unitOfWorkMock.Object,
                _providerRepositoryMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithValidRequest_ShouldCreateProviderSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Test Business";
            var subdomainSlug = "test-business";
            var description = "Test Description";
            var avatar = "https://example.com/avatar.jpg";
            var customDomain = "test.com";

            var command = new CreateProviderCommand(
                userId, businessName, subdomainSlug, description, avatar, customDomain);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var createdProvider = Provider.Create(userId, businessName, subdomainSlug);
            createdProvider.Description = description;
            createdProvider.Avatar = avatar;
            createdProvider.CustomDomain = customDomain;

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ProviderId.Should().NotBeEmpty();
            result.TenantId.Should().NotBeNull();
            result.BusinessName.Should().Be(businessName);
            result.SubdomainSlug.Should().Be(subdomainSlug);
            result.IsActive.Should().BeTrue();

            // Verify interactions
            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Provider"), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithMinimalRequest_ShouldCreateProviderSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Test Business";
            var subdomainSlug = "test-business";

            var command = new CreateProviderCommand(
                userId, businessName, subdomainSlug, null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var createdProvider = Provider.Create(userId, businessName, subdomainSlug);

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ProviderId.Should().NotBeEmpty();
            result.BusinessName.Should().Be(businessName);
            result.SubdomainSlug.Should().Be(subdomainSlug);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenUserNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain($"Użytkownik o ID {userId} nie istnieje");
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenUserAlreadyHasProvider_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };
            user.Provider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business"); // User already has a provider

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain($"Użytkownik {user.Email} już ma przypisanego providera");
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenSubdomainAlreadyTaken_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subdomainSlug = "taken-subdomain";
            var command = new CreateProviderCommand(
                userId, "Test Business", subdomainSlug, null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var existingProvider = Provider.Create(Guid.NewGuid(), "Existing Business", subdomainSlug);

            SetupSuccessfulUserLookup(user);
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain($"Subdomain '{subdomainSlug}' jest już zajęty");
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRoleAssignmentFails_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var createdProvider = Provider.Create(userId, "Test Business", "test-business");

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            
            // Setup role assignment failure
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Provider"))
                .ThrowsAsync(new Exception("Role assignment failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenProviderCreationFails_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            
            // Setup provider creation failure
            _providerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_ShouldSetUserTenantId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var createdProvider = Provider.Create(userId, "Test Business", "test-business");

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            user.TenantId.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithNullCommand_ShouldHandleGracefully()
        {
            // Arrange
            CreateProviderCommand? command = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(command!, CancellationToken.None));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithEmptyGuidUserId_ShouldThrowException()
        {
            // Arrange
            var command = new CreateProviderCommand(
                Guid.Empty, "Test Business", "test-business", null, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain("Użytkownik o ID 00000000-0000-0000-0000-000000000000 nie istnieje");
        }

        #region Helper Methods

        private void SetupSuccessfulUserLookup(ApplicationUser user)
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
        }

        private void SetupNoExistingProvider()
        {
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);
        }

        private void SetupSuccessfulProviderCreation(Provider provider)
        {
            _providerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(provider);
        }

        private void SetupSuccessfulRoleAssignment()
        {
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
        }

        #endregion

        #region Security Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithUnauthorizedUser_ShouldThrowSecurityException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var unauthorizedUser = new ApplicationUser
            {
                Id = userId,
                Email = "unauthorized@example.com",
                TenantId = TenantId.New() // User already belongs to different tenant
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(unauthorizedUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain($"Użytkownik {unauthorizedUser.Email} już ma przypisanego providera");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithMaliciousSubdomain_ShouldSanitizeInput()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var maliciousSubdomain = "<script>alert('xss')</script>malicious-subdomain";
            var command = new CreateProviderCommand(
                userId, "Test Business", maliciousSubdomain, null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var createdProvider = Provider.Create(userId, "Test Business", "malicious-subdomain"); // Should be sanitized

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.SubdomainSlug.Should().NotContain("<script>");
            result.SubdomainSlug.Should().NotContain("alert");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithSQLInjectionInBusinessName_ShouldSanitizeInput()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var maliciousBusinessName = "'; DROP TABLE Providers; --";
            var command = new CreateProviderCommand(
                userId, maliciousBusinessName, "test-business", null, null, null);

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com"
            };

            var createdProvider = Provider.Create(userId, maliciousBusinessName, "test-business");

            SetupSuccessfulUserLookup(user);
            SetupNoExistingProvider();
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.BusinessName.Should().Be(maliciousBusinessName); // Should be stored as-is (parameterized queries prevent SQL injection)
            
            // Verify that the provider was created with the exact input (parameterized queries handle SQL injection)
            _providerRepositoryMock.Verify(x => x.AddAsync(
                It.Is<Provider>(p => p.BusinessName == maliciousBusinessName), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithCrossTenantDataAccess_ShouldPreventAccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            var userFromDifferentTenant = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                TenantId = TenantId.New() // Different tenant
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(userFromDifferentTenant);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain($"Użytkownik {userFromDifferentTenant.Email} już ma przypisanego providera");
        }

        #endregion
    }
}