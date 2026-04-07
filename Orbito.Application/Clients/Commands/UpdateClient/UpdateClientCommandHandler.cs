using MediatR;
using Orbito.Application.Clients;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
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
            client.UpdateContactInfo(request.CompanyName, request.Phone);

            // Aktualizuj dane bezpośrednie (tylko dla klientów bez konta Identity)
            if (client.UserId == null)
            {
                var updateResult = client.UpdateDirectInfo(request.DirectEmail, request.DirectFirstName, request.DirectLastName);
                if (updateResult.IsFailure)
                {
                    return Result.Failure<ClientDto>(updateResult.Error);
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

            return Result.Success(ClientMapper.ToDto(updatedClient));
        }
    }
}
