using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;
using PaginatedList = Orbito.Application.Common.Models.PaginatedList<Orbito.Application.DTOs.ClientDto>;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    public class SearchClientsQueryHandler : IRequestHandler<SearchClientsQuery, Result<PaginatedList>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;

        public SearchClientsQueryHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
        }

        public async Task<Result<PaginatedList>> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<PaginatedList>(DomainErrors.Tenant.NoTenantContext);
            }

            // Walidacja search term
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Result.Failure<PaginatedList>(DomainErrors.Validation.Required("SearchTerm"));
            }

            // Wyszukaj klientów
            var clients = await _clientRepository.SearchClientsAsync(
                request.SearchTerm,
                request.PageNumber,
                request.PageSize,
                request.ActiveOnly,
                cancellationToken);

            var totalCount = await _clientRepository.GetSearchCountAsync(
                request.SearchTerm,
                request.ActiveOnly,
                cancellationToken);

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
