using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.GetClientById
{
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, GetClientByIdResult>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;

        public GetClientByIdQueryHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
        }

        public async Task<GetClientByIdResult> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return GetClientByIdResult.NotFoundResult("Tenant context is required");
                }

                // Pobierz klienta
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return GetClientByIdResult.NotFoundResult("Client not found");
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    return GetClientByIdResult.NotFoundResult("Access denied");
                }

                var clientDto = MapToDto(client);
                return GetClientByIdResult.SuccessResult(clientDto);
            }
            catch (Exception ex)
            {
                return GetClientByIdResult.NotFoundResult($"An error occurred while retrieving client: {ex.Message}");
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
