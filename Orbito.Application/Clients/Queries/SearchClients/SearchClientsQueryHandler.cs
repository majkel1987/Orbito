using Orbito.Application.DTOs;
using Orbito.Application.Common.Models;
using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    public class SearchClientsQueryHandler : IRequestHandler<SearchClientsQuery, Orbito.Domain.Common.Result<PaginatedList<ClientDto>>>
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

        public async Task<Orbito.Domain.Common.Result<PaginatedList<ClientDto>>> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return Orbito.Domain.Common.Result.Failure<PaginatedList<ClientDto>>(DomainErrors.Tenant.NoTenantContext);
            }

            // Walidacja search term
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                return Orbito.Domain.Common.Result.Failure<PaginatedList<ClientDto>>(DomainErrors.Validation.Required("SearchTerm"));
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

            var clientDtos = clients.Select(MapToDto).ToList();
            var paginatedList = new PaginatedList<ClientDto>(
                clientDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Orbito.Domain.Common.Result.Success(paginatedList);
        }

        private static ClientDto MapToDto(Orbito.Domain.Entities.Client client)
        {
            return new ClientDto
            {
                Id = client.Id,
                TenantId = client.TenantId.Value,
                UserId = client.UserId,
                CompanyName = client.CompanyName,
                Phone = client.Phone,
                DirectEmail = client.DirectEmail,
                DirectFirstName = client.DirectFirstName,
                DirectLastName = client.DirectLastName,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                Email = client.Email,
                FirstName = client.FirstName,
                LastName = client.LastName,
                FullName = client.FullName,
                UserEmail = client.User?.Email,
                UserFirstName = client.User?.FirstName,
                UserLastName = client.User?.LastName
            };
        }
    }
}
