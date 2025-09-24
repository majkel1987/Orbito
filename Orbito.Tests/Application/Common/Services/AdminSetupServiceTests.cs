using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Common.Services;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
using Xunit;

namespace Orbito.Tests.Application.Common.Services
{
    public class AdminSetupServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<AdminSetupService>> _loggerMock;
        private readonly AdminSetupService _adminSetupService;

        public AdminSetupServiceTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
            
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                Mock.Of<IRoleStore<ApplicationRole>>(), null!, null!, null!, null!);
            
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AdminSetupService>>();

            _adminSetupService = new AdminSetupService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        #region IsAdminSetupRequiredAsync Tests

        [Fact]
        public async Task IsAdminSetupRequiredAsync_WhenNoAdminUsersExist_ShouldReturnTrue()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _adminSetupService.IsAdminSetupRequiredAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAdminSetupRequiredAsync_WhenAdminUsersExist_ShouldReturnFalse()
        {
            // Arrange
            var adminUsers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = Guid.NewGuid(), Email = "admin@example.com" }
            };
            
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ReturnsAsync(adminUsers);

            // Act
            var result = await _adminSetupService.IsAdminSetupRequiredAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAdminSetupRequiredAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _adminSetupService.IsAdminSetupRequiredAsync();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region IsAdminSetupEnabledAsync Tests

        [Fact]
        public async Task IsAdminSetupEnabledAsync_WhenInDevelopmentEnvironment_ShouldReturnTrue()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Returns("Development");

            // Act
            var result = await _adminSetupService.IsAdminSetupEnabledAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAdminSetupEnabledAsync_WhenInProductionAndEnabledInConfig_ShouldReturnTrue()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Returns("Production");
            _configurationMock.Setup(x => x.GetValue<bool>("AdminSetup:Enabled", false))
                .Returns(true);

            // Act
            var result = await _adminSetupService.IsAdminSetupEnabledAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAdminSetupEnabledAsync_WhenInProductionAndDisabledInConfig_ShouldReturnFalse()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Returns("Production");
            _configurationMock.Setup(x => x.GetValue<bool>("AdminSetup:Enabled", false))
                .Returns(false);

            // Act
            var result = await _adminSetupService.IsAdminSetupEnabledAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAdminSetupEnabledAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Throws(new Exception("Configuration error"));

            // Act
            var result = await _adminSetupService.IsAdminSetupEnabledAsync();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CreateInitialAdminAsync Tests

        [Fact]
        public async Task CreateInitialAdminAsync_WhenSetupDisabled_ShouldReturnFalse()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Returns("Production");
            _configurationMock.Setup(x => x.GetValue<bool>("AdminSetup:Enabled", false))
                .Returns(false);

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateInitialAdminAsync_WhenAdminAlreadyExists_ShouldReturnFalse()
        {
            // Arrange
            SetupConfigurationForEnabledSetup();
            SetupAdminAlreadyExists();

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateInitialAdminAsync_WhenUserAlreadyExists_ShouldReturnFalse()
        {
            // Arrange
            SetupConfigurationForEnabledSetup();
            SetupNoAdminExists();
            SetupUserAlreadyExists("admin@example.com");

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateInitialAdminAsync_WhenUserCreationFails_ShouldReturnFalse()
        {
            // Arrange
            SetupConfigurationForEnabledSetup();
            SetupNoAdminExists();
            SetupUserDoesNotExist("admin@example.com");
            SetupUserCreationFails();

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateInitialAdminAsync_WhenAllConditionsMet_ShouldReturnTrue()
        {
            // Arrange
            SetupConfigurationForEnabledSetup();
            SetupNoAdminExists();
            SetupUserDoesNotExist("admin@example.com");
            SetupSuccessfulUserCreation();

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeTrue();
            
            // Verify user creation
            _userManagerMock.Verify(x => x.CreateAsync(
                It.Is<ApplicationUser>(u => 
                    u.Email == "admin@example.com" &&
                    u.FirstName == "John" &&
                    u.LastName == "Doe" &&
                    u.TenantId == null &&
                    u.IsActive == true),
                "Password123!"), Times.Once);
            
            // Verify role assignment
            _userManagerMock.Verify(x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                UserRole.PlatformAdmin.ToString()), Times.Once);
        }

        [Fact]
        public async Task CreateInitialAdminAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            SetupConfigurationForEnabledSetup();
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _adminSetupService.CreateInitialAdminAsync(
                "admin@example.com", "Password123!", "John", "Doe");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private void SetupConfigurationForEnabledSetup()
        {
            _configurationMock.Setup(x => x.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production"))
                .Returns("Development");
        }

        private void SetupAdminAlreadyExists()
        {
            var adminUsers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = Guid.NewGuid(), Email = "existing@example.com" }
            };
            
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ReturnsAsync(adminUsers);
        }

        private void SetupNoAdminExists()
        {
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString()))
                .ReturnsAsync(new List<ApplicationUser>());
        }

        private void SetupUserAlreadyExists(string email)
        {
            var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(existingUser);
        }

        private void SetupUserDoesNotExist(string email)
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((ApplicationUser?)null);
        }

        private void SetupUserCreationFails()
        {
            var failedResult = IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Password is too short"
            });
            
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);
        }

        private void SetupSuccessfulUserCreation()
        {
            var successResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(successResult);
            
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
        }

        #endregion
    }
}
