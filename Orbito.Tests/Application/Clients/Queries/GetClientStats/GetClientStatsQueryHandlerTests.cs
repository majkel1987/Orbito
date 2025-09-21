using FluentAssertions;
using Moq;
using Orbito.Application.Clients.Queries.GetClientStats;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;
using Xunit;

namespace Orbito.Tests.Application.Clients.Queries.GetClientStats
{
    public class GetClientStatsQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ITenantContext> _tenantContextMock;
        private readonly GetClientStatsQueryHandler _handler;
        private readonly TenantId _tenantId = TenantId.New();

        public GetClientStatsQueryHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _tenantContextMock = new Mock<ITenantContext>();

            _tenantContextMock.Setup(x => x.HasTenant).Returns(true);
            _tenantContextMock.Setup(x => x.CurrentTenantId).Returns(_tenantId);

            _handler = new GetClientStatsQueryHandler(
                _clientRepositoryMock.Object,
                _tenantContextMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidStats_ShouldReturnClientStats()
        {
            // Arrange
            var expectedStats = new ClientStats
            {
                TotalClients = 100,
                ActiveClients = 80,
                InactiveClients = 20,
                ClientsWithIdentity = 60,
                DirectClients = 40,
                ClientsWithActiveSubscriptions = 50,
                TotalRevenue = 15000.50m,
                Currency = "USD"
            };

            var query = new GetClientStatsQuery();

            _clientRepositoryMock.Setup(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Stats.Should().NotBeNull();
            result.Stats!.TotalClients.Should().Be(100);
            result.Stats.ActiveClients.Should().Be(80);
            result.Stats.InactiveClients.Should().Be(20);
            result.Stats.ClientsWithIdentity.Should().Be(60);
            result.Stats.DirectClients.Should().Be(40);
            result.Stats.ClientsWithActiveSubscriptions.Should().Be(50);
            result.Stats.TotalRevenue.Should().Be(15000.50m);
            result.Stats.Currency.Should().Be("USD");
            result.Stats.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _clientRepositoryMock.Verify(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithZeroStats_ShouldReturnZeroStats()
        {
            // Arrange
            var expectedStats = new ClientStats
            {
                TotalClients = 0,
                ActiveClients = 0,
                InactiveClients = 0,
                ClientsWithIdentity = 0,
                DirectClients = 0,
                ClientsWithActiveSubscriptions = 0,
                TotalRevenue = 0m,
                Currency = "USD"
            };

            var query = new GetClientStatsQuery();

            _clientRepositoryMock.Setup(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Stats.Should().NotBeNull();
            result.Stats!.TotalClients.Should().Be(0);
            result.Stats.ActiveClients.Should().Be(0);
            result.Stats.InactiveClients.Should().Be(0);
            result.Stats.ClientsWithIdentity.Should().Be(0);
            result.Stats.DirectClients.Should().Be(0);
            result.Stats.ClientsWithActiveSubscriptions.Should().Be(0);
            result.Stats.TotalRevenue.Should().Be(0m);
        }

        [Fact]
        public async Task Handle_WithoutTenantContext_ShouldReturnFailure()
        {
            // Arrange
            _tenantContextMock.Setup(x => x.HasTenant).Returns(false);
            var query = new GetClientStatsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Tenant context is required");
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailure()
        {
            // Arrange
            var query = new GetClientStatsQuery();

            _clientRepositoryMock.Setup(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("An error occurred while retrieving client stats");
        }

        [Fact]
        public async Task Handle_WithDifferentCurrency_ShouldReturnCorrectCurrency()
        {
            // Arrange
            var expectedStats = new ClientStats
            {
                TotalClients = 50,
                ActiveClients = 45,
                InactiveClients = 5,
                ClientsWithIdentity = 30,
                DirectClients = 20,
                ClientsWithActiveSubscriptions = 25,
                TotalRevenue = 25000.75m,
                Currency = "EUR"
            };

            var query = new GetClientStatsQuery();

            _clientRepositoryMock.Setup(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Stats.Should().NotBeNull();
            result.Stats!.Currency.Should().Be("EUR");
            result.Stats.TotalRevenue.Should().Be(25000.75m);
        }

        [Fact]
        public async Task Handle_WithLargeNumbers_ShouldHandleCorrectly()
        {
            // Arrange
            var expectedStats = new ClientStats
            {
                TotalClients = 1000000,
                ActiveClients = 950000,
                InactiveClients = 50000,
                ClientsWithIdentity = 800000,
                DirectClients = 200000,
                ClientsWithActiveSubscriptions = 900000,
                TotalRevenue = 999999999.99m,
                Currency = "USD"
            };

            var query = new GetClientStatsQuery();

            _clientRepositoryMock.Setup(x => x.GetClientStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Stats.Should().NotBeNull();
            result.Stats!.TotalClients.Should().Be(1000000);
            result.Stats.ActiveClients.Should().Be(950000);
            result.Stats.TotalRevenue.Should().Be(999999999.99m);
        }
    }
}
