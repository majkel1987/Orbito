using MediatR;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.SearchClients
{
    public record SearchClientsQuery(
        string SearchTerm,
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false
    ) : IRequest<SearchClientsResult>;

    public record SearchClientsResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IEnumerable<ClientDto> Clients { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }

        public static SearchClientsResult SuccessResult(
            IEnumerable<ClientDto> clients,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new SearchClientsResult
            {
                Success = true,
                Clients = clients,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public static SearchClientsResult FailureResult(string message)
        {
            return new SearchClientsResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
