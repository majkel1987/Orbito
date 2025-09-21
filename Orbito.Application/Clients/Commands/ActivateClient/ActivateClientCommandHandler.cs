using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Commands.ActivateClient
{
    public class ActivateClientCommandHandler : IRequestHandler<ActivateClientCommand, ActivateClientResult>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateClientCommandHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<ActivateClientResult> Handle(ActivateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return ActivateClientResult.FailureResult("Tenant context is required");
                }

                // Pobierz klienta
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return ActivateClientResult.FailureResult("Client not found");
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    return ActivateClientResult.FailureResult("Access denied");
                }

                // Sprawdź czy klient nie jest już aktywny
                if (client.IsActive)
                {
                    return ActivateClientResult.FailureResult("Client is already active");
                }

                // Aktywuj klienta
                client.Activate();

                // Zapisz zmiany
                await _clientRepository.UpdateAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Pobierz zaktualizowane dane
                var updatedClient = await _clientRepository.GetByIdAsync(client.Id, cancellationToken);
                if (updatedClient == null)
                {
                    return ActivateClientResult.FailureResult("Failed to retrieve updated client");
                }

                var clientDto = MapToDto(updatedClient);
                return ActivateClientResult.SuccessResult(clientDto);
            }
            catch (Exception ex)
            {
                return ActivateClientResult.FailureResult($"An error occurred while activating client: {ex.Message}");
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
