using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Clients.Queries.GetClientStats
{
    public class GetClientStatsQueryHandler : IRequestHandler<GetClientStatsQuery, GetClientStatsResult>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;

        public GetClientStatsQueryHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
        }

        public async Task<GetClientStatsResult> Handle(GetClientStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return GetClientStatsResult.FailureResult("Tenant context is required");
                }

                // Pobierz statystyki
                var stats = await _clientRepository.GetClientStatsAsync(cancellationToken);

                var statsDto = new ClientStatsDto
                {
                    TotalClients = stats.TotalClients,
                    ActiveClients = stats.ActiveClients,
                    InactiveClients = stats.InactiveClients,
                    ClientsWithIdentity = stats.ClientsWithIdentity,
                    DirectClients = stats.DirectClients,
                    ClientsWithActiveSubscriptions = stats.ClientsWithActiveSubscriptions,
                    TotalRevenue = stats.TotalRevenue,
                    Currency = stats.Currency,
                    LastUpdated = DateTime.UtcNow
                };

                return GetClientStatsResult.SuccessResult(statsDto);
            }
            catch (Exception ex)
            {
                return GetClientStatsResult.FailureResult($"An error occurred while retrieving client stats: {ex.Message}");
            }
        }
    }
}
