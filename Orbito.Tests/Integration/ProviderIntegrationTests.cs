using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.DTOs;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Application.Providers.Commands.UpdateProvider;
using Orbito.Application.Providers.Commands.DeleteProvider;
using Orbito.Application.Providers.Queries.GetProviderById;
using Orbito.Application.Providers.Queries.GetAllProviders;
using Orbito.Application.Providers.Queries.GetProviderByUserId;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Orbito.Domain.Identity;
using Xunit;

namespace Orbito.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ProviderIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProviderRepository> _providerRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CreateProviderCommandHandler>> _createLoggerMock;
        private readonly Mock<ILogger<UpdateProviderCommandHandler>> _updateLoggerMock;
        private readonly Mock<ILogger<DeleteProviderCommandHandler>> _deleteLoggerMock;
        private readonly Mock<ILogger<GetProviderByIdQueryHandler>> _getByIdLoggerMock;
        private readonly Mock<ILogger<GetAllProvidersQueryHandler>> _getAllLoggerMock;
        private readonly Mock<ILogger<GetProviderByUserIdQueryHandler>> _getByUserIdLoggerMock;
        private readonly TenantId _tenantId = TenantId.New();

        public ProviderIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Mock services
            _tenantContextMock = new Mock<ITenantContext>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _providerRepositoryMock = new Mock<IProviderRepository>();
            
            // Fix nullable reference warnings by using null! or proper mock setup
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), 
                null!, null!, null!, null!, null!, null!, null!, null!);
            
            _createLoggerMock = new Mock<ILogger<CreateProviderCommandHandler>>();
            _updateLoggerMock = new Mock<ILogger<UpdateProviderCommandHandler>>();
            _deleteLoggerMock = new Mock<ILogger<DeleteProviderCommandHandler>>();
            _getByIdLoggerMock = new Mock<ILogger<GetProviderByIdQueryHandler>>();
            _getAllLoggerMock = new Mock<ILogger<GetAllProvidersQueryHandler>>();
            _getByUserIdLoggerMock = new Mock<ILogger<GetProviderByUserIdQueryHandler>>();

            // Setup tenant context
            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            // Setup UnitOfWork default behavior - return success with 1 affected record
            var successResult = Orbito.Application.Common.Models.Result<int>.Success(1);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(successResult));

            // Setup UserManager mock
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new ApplicationUser
                {
                    Id = Guid.Parse(id),
                    Email = "test@example.com",
                    UserName = "test@example.com",
                    Provider = null // User doesn't have a provider yet
                });

            // Setup UserManager.GetRolesAsync to return empty list (no roles yet)
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string>());

            // Setup UserManager.AddToRoleAsync to succeed
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Register services
            services.AddSingleton(_tenantContextMock.Object);
            services.AddSingleton(_unitOfWorkMock.Object);
            services.AddSingleton(_providerRepositoryMock.Object);
            services.AddSingleton(_userManagerMock.Object);
            services.AddSingleton(_createLoggerMock.Object);
            services.AddSingleton(_updateLoggerMock.Object);
            services.AddSingleton(_deleteLoggerMock.Object);
            services.AddSingleton(_getByIdLoggerMock.Object);
            services.AddSingleton(_getAllLoggerMock.Object);
            services.AddSingleton(_getByUserIdLoggerMock.Object);

            _serviceProvider = services.BuildServiceProvider();
        }

        #region Create Provider Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateProvider_WithValidData_ShouldCreateProviderSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Test Business";
            var subdomainSlug = "test-business";
            var description = "Test business description";

            var command = new CreateProviderCommand(userId, businessName, subdomainSlug, description);

            var createdProvider = Provider.Create(userId, businessName, subdomainSlug);
            
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);
            _providerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProvider);

            var handler = new CreateProviderCommandHandler(
                _unitOfWorkMock.Object,
                _providerRepositoryMock.Object,
                _userManagerMock.Object,
                _createLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.ProviderId.Should().NotBeEmpty();
            result.Value.BusinessName.Should().Be(businessName);
            result.Value.SubdomainSlug.Should().Be(subdomainSlug);
            result.Value.IsActive.Should().BeTrue();

            _providerRepositoryMock.Verify(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateProvider_WithUnavailableSubdomain_ShouldReturnFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Test Business";
            var subdomainSlug = "taken-subdomain";
            var description = "Test business description";

            var command = new CreateProviderCommand(userId, businessName, subdomainSlug, description);

            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Provider.Create(Guid.NewGuid(), "Existing Business", subdomainSlug));

            var handler = new CreateProviderCommandHandler(
                _unitOfWorkMock.Object,
                _providerRepositoryMock.Object,
                _userManagerMock.Object,
                _createLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Provider.SubdomainAlreadyExists");

            _providerRepositoryMock.Verify(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Update Provider Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdateProvider_WithValidData_ShouldUpdateProviderSuccessfully()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var businessName = "Updated Business Name";
            var description = "Updated description";
            var subdomainSlug = "updated-subdomain";
            var customDomain = "updated-domain.com";

            var command = new UpdateProviderCommand(providerId, businessName, description, null, subdomainSlug, customDomain);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Original Business", "original-subdomain");
            existingProvider.Id = providerId;

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync(subdomainSlug, providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _providerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _updateLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(providerId);
            result.Value.BusinessName.Should().Be(businessName);
            result.Value.Description.Should().Be(description);
            result.Value.SubdomainSlug.Should().Be(subdomainSlug);
            result.Value.CustomDomain.Should().Be(customDomain);

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _providerRepositoryMock.Verify(x => x.IsSubdomainAvailableAsync(subdomainSlug, providerId, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task UpdateProvider_WithNonExistentProvider_ShouldThrowException()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var businessName = "Updated Business Name";
            var description = "Updated description";
            var subdomainSlug = "updated-subdomain";

            var command = new UpdateProviderCommand(providerId, businessName, description, subdomainSlug, null);

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);

            var handler = new UpdateProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _updateLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Provider.NotFound");

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.IsSubdomainAvailableAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
            _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Get Provider Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetProviderById_WithValidId_ShouldReturnProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var query = new GetProviderByIdQuery(providerId);

            var expectedProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            expectedProvider.Id = providerId;

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedProvider);

            var handler = new GetProviderByIdQueryHandler(_providerRepositoryMock.Object, _getByIdLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(providerId);
            result.Value.BusinessName.Should().Be("Test Business");
            result.Value.SubdomainSlug.Should().Be("test-business");

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetProviderById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var query = new GetProviderByIdQuery(providerId);

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);

            var handler = new GetProviderByIdQueryHandler(_providerRepositoryMock.Object, _getByIdLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error.Code.Should().Be("Provider.NotFound");

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAllProviders_WithPagination_ShouldReturnProvidersList()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var query = new GetAllProvidersQuery(pageNumber, pageSize);

            var providers = new List<Provider>
            {
                Provider.Create(Guid.NewGuid(), "Business 1", "business-1"),
                Provider.Create(Guid.NewGuid(), "Business 2", "business-2")
            };

            _providerRepositoryMock.Setup(x => x.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(providers);

            var handler = new GetAllProvidersQueryHandler(_providerRepositoryMock.Object, _getAllLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Providers.Should().HaveCount(2);
            result.Providers.Should().Contain(p => p.BusinessName == "Business 1");
            result.Providers.Should().Contain(p => p.BusinessName == "Business 2");

            _providerRepositoryMock.Verify(x => x.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetProviderByUserId_WithValidUserId_ShouldReturnProvider()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetProviderByUserIdQuery(userId);

            var expectedProvider = Provider.Create(userId, "Test Business", "test-business");

            _providerRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedProvider);

            var handler = new GetProviderByUserIdQueryHandler(_providerRepositoryMock.Object, _getByUserIdLoggerMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Provider.Should().NotBeNull();
            result.Provider!.UserId.Should().Be(userId);
            result.Provider.BusinessName.Should().Be("Test Business");

            _providerRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Delete Provider Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SoftDelete_ValidProvider_ShouldDeactivateProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, false);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            existingProvider.Id = providerId;

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HardDelete_ProviderWithoutDependencies_ShouldDeletePermanently()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, true);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            existingProvider.Id = providerId;

            // Mock CanBeDeleted to return true (no dependencies)
            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteProvider_NonExistentId_ShouldReturnFailure()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, false);

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Provider.NotFound");

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _providerRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteProvider_DatabaseError_ShouldReturnFailure()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, false);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            existingProvider.Id = providerId;

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act & Assert
            // Exception should propagate - handler doesn't catch exceptions anymore
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));

            _providerRepositoryMock.Verify(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SoftDelete_MultipleTimes_ShouldHandleGracefully()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, false);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            existingProvider.Id = providerId;
            existingProvider.Deactivate(); // Already deactivated

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert - Should still succeed even if already deactivated
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(Unit.Value);

            _providerRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task DeleteProvider_SaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var command = new DeleteProviderCommand(providerId, false);

            var existingProvider = Provider.Create(Guid.NewGuid(), "Test Business", "test-business");
            existingProvider.Id = providerId;

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProvider);
            _providerRepositoryMock.Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup UnitOfWork to return failure
            var failureResult = Orbito.Application.Common.Models.Result<int>.Failure("Database save failed", "SaveFailed");
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(failureResult));

            var handler = new DeleteProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _deleteLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("Provider.DeleteFailed");
            result.Error.Message.Should().Contain("Database save failed");

            _providerRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Business Logic Integration Tests

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ProviderBusinessLogic_ShouldHandleComplexScenarios()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Complex Business";
            var subdomainSlug = "complex-business";
            var description = "Complex business with multiple features";

            // Test 1: Create Provider
            var createCommand = new CreateProviderCommand(userId, businessName, subdomainSlug, description);
            var createdProvider = Provider.Create(userId, businessName, subdomainSlug);
            
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Provider?)null);
            _providerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProvider);

            var createHandler = new CreateProviderCommandHandler(
                _unitOfWorkMock.Object,
                _providerRepositoryMock.Object,
                _userManagerMock.Object,
                _createLoggerMock.Object);

            // Act
            var createResult = await createHandler.Handle(createCommand, CancellationToken.None);

            // Assert
            createResult.Should().NotBeNull();
            createResult.IsSuccess.Should().BeTrue();
            createResult.Value.ProviderId.Should().NotBeEmpty();
            createResult.Value.BusinessName.Should().Be(businessName);
            createResult.Value.SubdomainSlug.Should().Be(subdomainSlug);

            // Test 2: Update Provider with business logic
            var providerId = createdProvider.Id;
            var updateCommand = new UpdateProviderCommand(
                providerId, 
                "Updated Complex Business", 
                "Updated description", 
                null,
                "updated-complex-business", 
                "complex-business.com");

            _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdProvider);
            _providerRepositoryMock.Setup(x => x.IsSubdomainAvailableAsync("updated-complex-business", providerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var updateHandler = new UpdateProviderCommandHandler(
                _providerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _updateLoggerMock.Object);

            // Act
            var updateResult = await updateHandler.Handle(updateCommand, CancellationToken.None);

            // Assert
            updateResult.Should().NotBeNull();
            updateResult.IsSuccess.Should().BeTrue();
            updateResult.Value.Should().NotBeNull();
            updateResult.Value!.Id.Should().Be(providerId);
            updateResult.Value.BusinessName.Should().Be("Updated Complex Business");
            updateResult.Value.CustomDomain.Should().Be("complex-business.com");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task ProviderValidation_ShouldHandleEdgeCases()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var businessName = "Edge Case Business";
            var subdomainSlug = "edge-case-business";
            var description = "Business with edge case scenarios";

            var command = new CreateProviderCommand(userId, businessName, subdomainSlug, description);

            // Simulate database error
            _providerRepositoryMock.Setup(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection error"));

            var handler = new CreateProviderCommandHandler(
                _unitOfWorkMock.Object,
                _providerRepositoryMock.Object,
                _userManagerMock.Object,
                _createLoggerMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("General.UnexpectedError");

            _providerRepositoryMock.Verify(x => x.GetBySubdomainSlugAsync(subdomainSlug, It.IsAny<CancellationToken>()), Times.Once);
            _providerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}