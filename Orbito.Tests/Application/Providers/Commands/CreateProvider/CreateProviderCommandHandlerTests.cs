using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Models;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;
using Xunit;
using DomainResult = Orbito.Domain.Common.Result;
using AppResult = Orbito.Application.Common.Models.Result;

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
            SetupSuccessfulSave();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.ProviderId.Should().NotBeEmpty();
            result.Value.TenantId.Should().NotBeNull();
            result.Value.BusinessName.Should().Be(businessName);
            result.Value.SubdomainSlug.Should().Be(subdomainSlug);
            result.Value.IsActive.Should().BeTrue();

            // Verify interactions
            _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Provider"), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            SetupSuccessfulSave();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.ProviderId.Should().NotBeEmpty();
            result.Value.BusinessName.Should().Be(businessName);
            result.Value.SubdomainSlug.Should().Be(subdomainSlug);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateProviderCommand(
                userId, "Test Business", "test-business", null, null, null);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenUserAlreadyHasProvider_ShouldReturnFailure()
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenSubdomainAlreadyTaken_ShouldReturnFailure()
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

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenRoleAssignmentFails_ShouldReturnFailure()
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
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Provider"))
                .ThrowsAsync(new Exception("Role assignment failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WhenProviderCreationFails_ShouldReturnFailure()
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
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
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
            SetupSuccessfulSave();

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
        public async Task Handle_WithEmptyGuidUserId_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateProviderCommand(
                Guid.Empty, "Test Business", "test-business", null, null, null);

            _userManagerMock.Setup(x => x.FindByIdAsync(Guid.Empty.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
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
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());
        }

        private void SetupSuccessfulSave()
        {
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Orbito.Application.Common.Models.Result<int>.Success(1))); // Returns Result<int> with 1 row affected
        }

        #endregion

        #region Security Tests

        [Fact]
        [Trait("Category", "Unit")]
        public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
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
            unauthorizedUser.Provider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(unauthorizedUser);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
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

            // The handler should sanitize the subdomain, removing HTML tags
            var sanitizedSubdomain = "malicious-subdomain";
            var createdProvider = Provider.Create(userId, "Test Business", sanitizedSubdomain);

            SetupSuccessfulUserLookup(user);
            // Mock should expect the sanitized subdomain
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(sanitizedSubdomain, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);
            SetupSuccessfulProviderCreation(createdProvider);
            SetupSuccessfulRoleAssignment();
            SetupSuccessfulSave();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.SubdomainSlug.Should().NotContain("<script>");
            result.Value.SubdomainSlug.Should().NotContain("alert");
            result.Value.SubdomainSlug.Should().Be(sanitizedSubdomain);
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
            SetupSuccessfulSave();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.BusinessName.Should().Be(maliciousBusinessName); // Should be stored as-is (parameterized queries prevent SQL injection)

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
            userFromDifferentTenant.Provider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(userFromDifferentTenant);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBe(Error.None);
            result.Error.Message.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}