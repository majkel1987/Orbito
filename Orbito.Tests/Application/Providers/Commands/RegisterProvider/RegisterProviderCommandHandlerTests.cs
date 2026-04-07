using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Application.Providers.Commands.RegisterProvider;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Commands.RegisterProvider;

[Trait("Category", "Unit")]
public class RegisterProviderCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<RegisterProviderCommandHandler>> _mockLogger;
    private readonly RegisterProviderCommandHandler _handler;

    public RegisterProviderCommandHandlerTests()
    {
        // Create mocks for UserManager dependencies
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        // Create mocks for RoleManager dependencies
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<RegisterProviderCommandHandler>>();

        _handler = new RegisterProviderCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidRegistration_ShouldCreateProviderAndUser()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "newprovider@test.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "Test Business",
            "testbusiness",
            null,
            "Test Description",
            null,
            null
        );

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var subdomainAvailability = new SubdomainAvailabilityResult
        {
            IsAvailable = true,
            Message = "Subdomain available"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CheckSubdomainAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subdomainAvailability);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());
        var createProviderResult = Result.Success(new CreateProviderResult(
            providerId,
            tenantId,
            "Test Business",
            "testbusiness",
            true
        ));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createProviderResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.UserId.Should().NotBeEmpty();
        result.ProviderId.Should().NotBeEmpty();

        _mockUserManager.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "existing@test.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "Test Business",
            "testbusiness"
        );

        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            UserName = "existing@test.com"
        };

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("już istnieje");

        _mockUserManager.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSubdomain_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "newprovider@test.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "Test Business",
            "existingslug"
        );

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var subdomainAvailability = new SubdomainAvailabilityResult
        {
            IsAvailable = false,
            Message = "Subdomain already taken"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CheckSubdomainAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subdomainAvailability);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("użyciu");

        _mockUserManager.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "newprovider@test.com",
            "WeakPassword",
            "John",
            "Doe",
            "Test Business",
            "testbusiness"
        );

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var subdomainAvailability = new SubdomainAvailabilityResult
        {
            IsAvailable = true,
            Message = "Subdomain available"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CheckSubdomainAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subdomainAvailability);

        var identityErrors = new[]
        {
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" }
        };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Password is too short");

        _mockMediator.Verify(
            x => x.Send(It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ProviderCreationFails_ShouldDeleteUserAndReturnFailure()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "newprovider@test.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "Test Business",
            "testbusiness"
        );

        var createdUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            UserName = command.Email
        };

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var subdomainAvailability = new SubdomainAvailabilityResult
        {
            IsAvailable = true,
            Message = "Subdomain available"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CheckSubdomainAvailabilityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(subdomainAvailability);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, password) =>
            {
                // Simulate user created
                user.Id = createdUser.Id;
            });

        var createProviderResult = Result.Failure<CreateProviderResult>(
            Error.Create("Provider.CreationFailed", "Failed to create provider")
        );

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateProviderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createProviderResult);

        _mockUserManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();

        _mockUserManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterProviderCommand(
            "newprovider@test.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "Test Business",
            "testbusiness"
        );

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();

        // Logger verification removed
    }
}
