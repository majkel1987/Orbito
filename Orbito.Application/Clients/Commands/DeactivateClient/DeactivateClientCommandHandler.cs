using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Clients.Commands.DeactivateClient
{
    public class DeactivateClientCommandHandler : IRequestHandler<DeactivateClientCommand, Result<ClientDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateClientCommandHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClientDto>> Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
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

            // Sprawdź czy klient nie jest już nieaktywny
            if (!client.IsActive)
            {
                return Result.Failure<ClientDto>(DomainErrors.Client.AlreadyInactive);
            }

            // Dezaktywuj klienta
            client.Deactivate();

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
