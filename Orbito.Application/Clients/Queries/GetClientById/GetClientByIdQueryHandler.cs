using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Clients.Queries.GetClientById
{
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
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

        public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            // Sprawdź czy mamy kontekst tenanta
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<ClientDto>(DomainErrors.Tenant.NoTenantContext);
            }

            // Pobierz klienta (repository already filters by TenantId)
            var client = await _clientRepository.GetByIdAsync(request.Id, cancellationToken);
            if (client == null)
            {
                return Result.Failure<ClientDto>(DomainErrors.Client.NotFound);
            }

            // Additional security check: verify tenant ownership
            // Note: Repository already filters by TenantId, but this provides defense in depth
            if (client.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Failure<ClientDto>(DomainErrors.Tenant.CrossTenantAccess);
            }

            return Result.Success(ClientMapper.ToDto(client));
        }
    }
}
