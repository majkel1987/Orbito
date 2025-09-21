using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    public class SearchClientsQueryHandler : IRequestHandler<SearchClientsQuery, SearchClientsResult>
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

        public async Task<SearchClientsResult> Handle(SearchClientsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return SearchClientsResult.FailureResult("Tenant context is required");
                }

                // Walidacja search term
                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    return SearchClientsResult.FailureResult("Search term is required");
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

                return SearchClientsResult.SuccessResult(
                    clientDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                return SearchClientsResult.FailureResult($"An error occurred while searching clients: {ex.Message}");
            }
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
