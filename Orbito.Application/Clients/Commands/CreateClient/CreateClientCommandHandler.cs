using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Entities;
using Orbito.Domain.Errors;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Clients.Commands.CreateClient
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientCommandHandler(
            IClientRepository clientRepository,
            IProviderRepository providerRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _providerRepository = providerRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            // SECURITY: Verify tenant context
            if (!_tenantContext.HasTenant)
            {
                return Result.Failure<ClientDto>(DomainErrors.Tenant.NoTenantContext);
            }

            var tenantId = _tenantContext.CurrentTenantId!;

            // Verify Provider exists for this TenantId (Provider.Id == TenantId.Value)
            // This is required because Client.Provider navigation property must be set
            var provider = await _providerRepository.GetByIdAsync(tenantId.Value, cancellationToken);
            if (provider == null)
            {
                return Result.Failure<ClientDto>(DomainErrors.Provider.NotFound);
            }

            // Normalize request: UserId and DirectEmail are mutually exclusive (XOR)
            // If both are provided, prioritize UserId and clear Direct fields
            // This provides defense in depth if frontend sends invalid data
            var normalizedRequest = request;
            if (request.UserId.HasValue && !string.IsNullOrWhiteSpace(request.DirectEmail))
            {
                // Frontend should prevent this, but backend ensures data integrity
                // Prioritize UserId and clear Direct fields
                normalizedRequest = request with
                {
                    DirectEmail = null,
                    DirectFirstName = null,
                    DirectLastName = null
                };
            }

            // Validation is handled by CreateClientCommandValidator through ValidationBehaviour
            // Sprawdź czy klient już istnieje (po emailu)
            if (!string.IsNullOrWhiteSpace(normalizedRequest.DirectEmail))
            {
                var existingClient = await _clientRepository.GetByEmailAsync(normalizedRequest.DirectEmail, cancellationToken);
                if (existingClient != null)
                {
                    return Result.Failure<ClientDto>(DomainErrors.Client.EmailAlreadyExists);
                }
            }

            // Sprawdź czy klient już istnieje (po UserId)
            if (normalizedRequest.UserId.HasValue)
            {
                var existingClient = await _clientRepository.GetByUserIdAsync(normalizedRequest.UserId.Value, cancellationToken);
                if (existingClient != null)
                {
                    return Result.Failure<ClientDto>(DomainErrors.Client.UserAlreadyExists);
                }
            }

            // Utwórz klienta
            Client client;
            if (normalizedRequest.UserId.HasValue)
            {
                client = Client.CreateWithUser(
                    tenantId,
                    normalizedRequest.UserId.Value,
                    normalizedRequest.CompanyName);
            }
            else
            {
                client = Client.CreateDirect(
                    tenantId,
                    normalizedRequest.DirectEmail!,
                    normalizedRequest.DirectFirstName ?? "",
                    normalizedRequest.DirectLastName ?? "",
                    normalizedRequest.CompanyName);
            }

            // Ustaw dodatkowe właściwości
            if (!string.IsNullOrWhiteSpace(normalizedRequest.Phone))
            {
                client.Phone = normalizedRequest.Phone;
            }

            // Set Provider navigation property (required by EF Core relationship)
            // Provider.Id == TenantId.Value (self-referencing)
            client.Provider = provider;

            // Zapisz klienta
            // Note: AddAsync already calls SaveChangesAsync internally
            var createdClient = await _clientRepository.AddAsync(client, cancellationToken);

            // Pobierz pełne dane klienta
            var clientWithDetails = await _clientRepository.GetByIdAsync(createdClient.Id, cancellationToken);
            if (clientWithDetails == null)
            {
                return Result.Failure<ClientDto>(DomainErrors.Client.NotFound);
            }

            var clientDto = MapToDto(clientWithDetails);
            return Result.Success(clientDto);
        }

        private static ClientDto MapToDto(Client client)
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
