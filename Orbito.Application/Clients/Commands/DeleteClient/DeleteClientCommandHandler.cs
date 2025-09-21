using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Clients.Commands.DeleteClient
{
    public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, DeleteClientResult>
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

        public async Task<DeleteClientResult> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return DeleteClientResult.FailureResult("Tenant context is required");
                }

                // Pobierz klienta
                var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
                if (client == null)
                {
                    return DeleteClientResult.FailureResult("Client not found");
                }

                // Sprawdź czy klient należy do tego samego tenanta
                if (client.TenantId != _tenantContext.CurrentTenantId)
                {
                    return DeleteClientResult.FailureResult("Access denied");
                }

                // Sprawdź czy klient może być usunięty
                if (!await _clientRepository.CanClientBeDeletedAsync(request.Id, cancellationToken))
                {
                    return DeleteClientResult.FailureResult("Client cannot be deleted because it has active subscriptions or payments");
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

                return DeleteClientResult.SuccessResult();
            }
            catch (Exception ex)
            {
                return DeleteClientResult.FailureResult($"An error occurred while deleting client: {ex.Message}");
            }
        }
    }
}
