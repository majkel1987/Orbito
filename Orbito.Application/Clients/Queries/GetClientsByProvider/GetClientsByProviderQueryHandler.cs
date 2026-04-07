using MediatR;
using Orbito.Application.Clients;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using PaginatedList = Orbito.Application.Common.Models.PaginatedList<Orbito.Application.DTOs.ClientDto>;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    public class GetClientsByProviderQueryHandler : IRequestHandler<GetClientsByProviderQuery, Result<PaginatedList>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;

        public GetClientsByProviderQueryHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
        }

        public async Task<Result<PaginatedList>> Handle(GetClientsByProviderQuery request, CancellationToken cancellationToken)
        {
            // Defense in depth: sprawdź czy mamy kontekst tenanta
            // Query filters w ApplicationDbContext automatycznie filtrują po TenantId,
            // ale jawna weryfikacja zapewnia bezpieczeństwo w przypadku błędnej konfiguracji
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<PaginatedList>(DomainErrors.Tenant.NoTenantContext);
            }

            // Pobierz klientów na podstawie statusu
            IEnumerable<Orbito.Domain.Entities.Client> clients;
            int totalCount;

            if (request.ActiveOnly == true)
            {
                // Only active clients
                clients = await _clientRepository.GetActiveClientsAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    cancellationToken);
                totalCount = await _clientRepository.GetActiveClientsCountAsync(request.SearchTerm, cancellationToken);
            }
            else if (request.ActiveOnly == false)
            {
                // Only inactive clients
                clients = await _clientRepository.GetInactiveClientsAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    cancellationToken);
                totalCount = await _clientRepository.GetInactiveClientsCountAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                // All clients (ActiveOnly is null)
                clients = await _clientRepository.GetAllAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    cancellationToken);
                totalCount = await _clientRepository.GetTotalCountAsync(request.SearchTerm, cancellationToken);
            }

            var clientDtos = ClientMapper.ToDto(clients);
            var paginatedList = new PaginatedList(
                clientDtos.ToList(),
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result.Success(paginatedList);
        }
    }
}
