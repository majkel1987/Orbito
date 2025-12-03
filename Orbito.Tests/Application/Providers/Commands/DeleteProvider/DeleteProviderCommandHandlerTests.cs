using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.DeleteProvider;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Providers.Commands.DeleteProvider;

[Trait("Category", "Unit")]
public class DeleteProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<DeleteProviderCommandHandler>> _mockLogger;
    private readonly DeleteProviderCommandHandler _handler;

    public DeleteProviderCommandHandlerTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<DeleteProviderCommandHandler>>();

        _handler = new DeleteProviderCommandHandler(
            _mockProviderRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidSoftDelete_ShouldDeactivateProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockProviderRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = true, Value = 1 }));

        var command = new DeleteProviderCommand(providerId, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _mockProviderRepository.Verify(
            x => x.SoftDeleteAsync(provider, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockProviderRepository.Verify(
            x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidHardDelete_ShouldPermanentlyDelete()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockProviderRepository
            .Setup(x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = true, Value = 1 }));

        var command = new DeleteProviderCommand(providerId, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _mockProviderRepository.Verify(
            x => x.DeleteAsync(provider, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockProviderRepository.Verify(
            x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        // Logger verification removed
    }

    [Fact]
    public async Task Handle_NonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var command = new DeleteProviderCommand(providerId, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.NotFound");

        _mockProviderRepository.Verify(
            x => x.DeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockProviderRepository.Verify(
            x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_HardDeleteWithActiveClients_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var tenantId = TenantId.Create(Guid.NewGuid());

        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "Test Provider",
            "testprovider");

        // Set active clients count to simulate provider with active clients
        provider.UpdateActiveClientsCount(5); // Provider has 5 active clients

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var command = new DeleteProviderCommand(providerId, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - provider has active clients, so hard delete should fail
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.CannotDeleteWithActiveClients");
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
            "Test Provider",
            "testprovider");

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockProviderRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Orbito.Application.Common.Models.Result<int> { IsSuccess = false, ErrorMessage = "Database error", ErrorCode = "Database.Error" }));

        var command = new DeleteProviderCommand(providerId, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Provider.DeleteFailed");
        result.Error.Message.Should().Contain("Database error");

        // Logger verification removed
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _mockProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var command = new DeleteProviderCommand(providerId, false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
