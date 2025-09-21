using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, UpdateClientResult>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClientCommandHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateClientResult> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return UpdateClientResult.FailureResult("Tenant context is required");
                }

                // Pobierz klienta
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return UpdateClientResult.FailureResult("Client not found");
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    return UpdateClientResult.FailureResult("Access denied");
                }

                // Sprawdź czy email nie jest już używany przez innego klienta
                if (!string.IsNullOrWhiteSpace(request.DirectEmail) && 
                    request.DirectEmail != client.DirectEmail)
                {
                    var existingClient = await _clientRepository.GetByEmailAsync(request.DirectEmail, cancellationToken);
                    if (existingClient != null && existingClient.Id != request.Id)
                    {
                        return UpdateClientResult.FailureResult("Email is already used by another client");
                    }
                }

                // Aktualizuj właściwości klienta
                if (!string.IsNullOrWhiteSpace(request.CompanyName))
                {
                    client.CompanyName = request.CompanyName;
                }

                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    client.Phone = request.Phone;
                }

                // Aktualizuj dane bezpośrednie (tylko dla klientów bez konta Identity)
                if (client.UserId == null)
                {
                    if (!string.IsNullOrWhiteSpace(request.DirectEmail))
                    {
                        client.DirectEmail = request.DirectEmail;
                    }

                    if (!string.IsNullOrWhiteSpace(request.DirectFirstName))
                    {
                        client.DirectFirstName = request.DirectFirstName;
                    }

                    if (!string.IsNullOrWhiteSpace(request.DirectLastName))
                    {
                        client.DirectLastName = request.DirectLastName;
                    }
                }

                // Zapisz zmiany
                await _clientRepository.UpdateAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Pobierz zaktualizowane dane
                var updatedClient = await _clientRepository.GetByIdAsync(client.Id, cancellationToken);
                if (updatedClient == null)
                {
                    return UpdateClientResult.FailureResult("Failed to retrieve updated client");
                }

                var clientDto = MapToDto(updatedClient);
                return UpdateClientResult.SuccessResult(clientDto);
            }
            catch (Exception ex)
            {
                return UpdateClientResult.FailureResult($"An error occurred while updating client: {ex.Message}");
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
