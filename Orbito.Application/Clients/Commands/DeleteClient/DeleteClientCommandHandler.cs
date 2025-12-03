using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Clients.Commands.DeleteClient
{
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Result<Unit>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteClientCommandHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<Unit>(DomainErrors.Tenant.NoTenantContext);
            }

            // Pobierz klienta
            var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (client == null)
            {
                return Result.Failure<Unit>(DomainErrors.Client.NotFound);
            }

            // Sprawdź czy klient należy do tego samego tenanta
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Failure<Unit>(DomainErrors.Tenant.CrossTenantAccess);
            }

            // Sprawdź czy klient może być usunięty
            if (!await _clientRepository.CanClientBeDeletedAsync(request.Id, cancellationToken))
            {
                return Result.Failure<Unit>(DomainErrors.Client.CannotDeleteWithActiveSubscriptions);
            }

            // Usuń klienta
            if (request.HardDelete)
            {
                await _clientRepository.DeleteAsync(client, cancellationToken);
            }
            else
            {
                await _clientRepository.SoftDeleteAsync(client, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
    }
}
