using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Clients.Commands.CreateClient
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, CreateClientResult>
    {
        private readonly IClientRepository _clientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public CreateClientCommandHandler(
            IClientRepository clientRepository,
            ITenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _clientRepository = clientRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateClientResult> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy mamy kontekst tenanta
                if (!_tenantContext.HasTenant)
                {
                    return CreateClientResult.FailureResult("Tenant context is required");
                }

                var tenantId = _tenantContext.CurrentTenantId!;

                // Walidacja - musi być podany UserId lub dane bezpośrednie
                if (!request.UserId.HasValue && string.IsNullOrWhiteSpace(request.DirectEmail))
                {
                    return CreateClientResult.FailureResult("Either UserId or DirectEmail must be provided");
                }

                // Sprawdź czy klient już istnieje (po emailu)
                if (!string.IsNullOrWhiteSpace(request.DirectEmail))
                {
                    var existingClient = await _clientRepository.GetByEmailAsync(request.DirectEmail, cancellationToken);
                    if (existingClient != null)
                    {
                        return CreateClientResult.FailureResult("Client with this email already exists");
                    }
                }

                // Sprawdź czy klient już istnieje (po UserId)
                if (request.UserId.HasValue)
                {
                    var existingClient = await _clientRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
                    if (existingClient != null)
                    {
                        return CreateClientResult.FailureResult("Client with this user already exists");
                    }
                }

                // Utwórz klienta
                Client client;
                if (request.UserId.HasValue)
                {
                    client = Client.CreateWithUser(
                        tenantId,
                        request.UserId.Value,
                        request.CompanyName);
                }
                else
                {
                    client = Client.CreateDirect(
                        tenantId,
                        request.DirectEmail!,
                        request.DirectFirstName ?? "",
                        request.DirectLastName ?? "",
                        request.CompanyName);
                }

                // Ustaw dodatkowe właściwości
                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    client.Phone = request.Phone;
                }

                // Zapisz klienta
                var createdClient = await _clientRepository.AddAsync(client, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Pobierz pełne dane klienta
                var clientWithDetails = await _clientRepository.GetByIdAsync(createdClient.Id, cancellationToken);
                if (clientWithDetails == null)
                {
                    return CreateClientResult.FailureResult("Failed to retrieve created client");
                }

                var clientDto = MapToDto(clientWithDetails);
                return CreateClientResult.SuccessResult(clientDto);
            }
            catch (Exception ex)
            {
                return CreateClientResult.FailureResult($"An error occurred while creating client: {ex.Message}");
            }
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
