using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.UpdateProvider;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Commands.UpdateProvider;

[Trait("Category", "Unit")]
public class UpdateProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<UpdateProviderCommandHandler>> _mockLogger;
    private readonly UpdateProviderCommandHandler _handler;

    public UpdateProviderCommandHandlerTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UpdateProviderCommandHandler>>();

        _handler = new UpdateProviderCommandHandler(
            _mockProviderRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldUpdateProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Old Name",
            "oldslug");

        var updatedProvider = Provider.Create(
            userId,
            "New Provider Name",
            "oldslug");

        _mockProviderRepository
            .SetupSequence(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider)
            .ReturnsAsync(updatedProvider);

        _mockProviderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = true, Value = 1 }));

        var command = new UpdateProviderCommand(
            providerId,
            "New Provider Name",
            "New Description",
            null,
            "oldslug"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BusinessName.Should().Be("New Provider Name");

        _mockProviderRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var command = new UpdateProviderCommand(providerId, "New Provider Name");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.NotFound");

        _mockProviderRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateSubdomain_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Provider Name",
            "oldslug");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockProviderRepository
            .Setup(x => x.IsSubdomainAvailableAsync("newslug", providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateProviderCommand(
            providerId,
            provider.BusinessName,
            null,
            null,
            "newslug"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.SubdomainAlreadyExists");

        _mockProviderRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ChangeSubdomain_ShouldCheckAvailability()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Provider Name",
            "oldslug");

        var updatedProvider = Provider.Create(
            userId,
            "Provider Name",
            "newslug");

        _mockProviderRepository
            .SetupSequence(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider)
            .ReturnsAsync(updatedProvider);

        _mockProviderRepository
            .Setup(x => x.IsSubdomainAvailableAsync("newslug", providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockProviderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = true, Value = 1 }));

        var command = new UpdateProviderCommand(
            providerId,
            provider.BusinessName,
            null,
            null,
            "newslug"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockProviderRepository.Verify(
            x => x.IsSubdomainAvailableAsync("newslug", providerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Provider Name",
            "testslug");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockProviderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = false, ErrorMessage = "Database error", ErrorCode = "Database.Error" }));

        var command = new UpdateProviderCommand(providerId, "New Name");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.SaveFailed");
        result.Error.Message.Should().Contain("Database error");
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var command = new UpdateProviderCommand(providerId, "New Name");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
