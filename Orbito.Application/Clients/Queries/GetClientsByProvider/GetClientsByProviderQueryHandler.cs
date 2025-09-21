using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    public class GetClientsByProviderQueryHandler : IRequestHandler<GetClientsByProviderQuery, GetClientsByProviderResult>
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

        public async Task<GetClientsByProviderResult> Handle(GetClientsByProviderQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return GetClientsByProviderResult.FailureResult("Tenant context is required");
                }

                // Pobierz klientów
                IEnumerable<Orbito.Domain.Entities.Client> clients;
                int totalCount;

                if (request.ActiveOnly)
                {
                    clients = await _clientRepository.GetActiveClientsAsync(
                        request.PageNumber,
                        request.PageSize,
                        request.SearchTerm,
                        cancellationToken);
                    totalCount = await _clientRepository.GetActiveClientsCountAsync(request.SearchTerm, cancellationToken);
                }
                else
                {
                    clients = await _clientRepository.GetAllAsync(
                        request.PageNumber,
                        request.PageSize,
                        request.SearchTerm,
                        cancellationToken);
                    totalCount = await _clientRepository.GetTotalCountAsync(request.SearchTerm, cancellationToken);
                }

                var clientDtos = clients.Select(MapToDto).ToList();

                return GetClientsByProviderResult.SuccessResult(
                    clientDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                return GetClientsByProviderResult.FailureResult($"An error occurred while retrieving clients: {ex.Message}");
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
