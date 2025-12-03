using Orbito.Application.DTOs;
using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
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

        public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<ClientDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // Pobierz klienta
            var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (client == null)
            {
                return Result.Failure<ClientDto>(DomainErrors.Client.NotFound);
            }

            // Sprawdź czy klient należy do tego samego tenanta
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Failure<ClientDto>(DomainErrors.Tenant.CrossTenantAccess);
            }

            // Sprawdź czy email nie jest już używany przez innego klienta
            if (!string.IsNullOrWhiteSpace(request.DirectEmail) &&
                request.DirectEmail != client.DirectEmail)
            {
                var existingClient = await _clientRepository.GetByEmailAsync(request.DirectEmail, cancellationToken);
                if (existingClient != null && existingClient.Id != request.Id)
                {
                    return Result.Failure<ClientDto>(DomainErrors.Client.EmailAlreadyExists);
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
                return Result.Failure<ClientDto>(DomainErrors.Client.NotFound);
            }

            var clientDto = MapToDto(updatedClient);
            return Result.Success(clientDto);
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
