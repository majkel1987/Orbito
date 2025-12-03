using Orbito.Application.DTOs;
using Orbito.Application.Common.Models;
using MediatR;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    public class GetClientsByProviderQueryHandler : IRequestHandler<GetClientsByProviderQuery, Orbito.Domain.Common.Result<PaginatedList<ClientDto>>>
    {
        private readonly IClientRepository _clientRepository;

        public GetClientsByProviderQueryHandler(
            IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Orbito.Domain.Common.Result<PaginatedList<ClientDto>>> Handle(GetClientsByProviderQuery request, CancellationToken cancellationToken)
        {
            // Query filters w ApplicationDbContext automatycznie obsługują filtrowanie po TenantId
            // Jeśli nie ma TenantId, query filters zwrócą puste wyniki (bezpieczne zachowanie)

            // Pobierz klientów
            IEnumerable<Orbito.Domain.Entities.Client> clients;
            int totalCount;

            if (request.ActiveOnly)
            {
                clients = await _clientRepository.GetActiveClientsAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    cancellationToken);
                totalCount = await _clientRepository.GetActiveClientsCountAsync(request.SearchTerm, cancellationToken);
            }
            else
            {
                clients = await _clientRepository.GetAllAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm,
                    cancellationToken);
                totalCount = await _clientRepository.GetTotalCountAsync(request.SearchTerm, cancellationToken);
            }

            var clientDtos = clients.Select(MapToDto).ToList();
            var paginatedList = new PaginatedList<ClientDto>(
                clientDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Orbito.Domain.Common.Result.Success(paginatedList);
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
