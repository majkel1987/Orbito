using MediatR;
using Orbito.Application.Clients.Commands.CreateClient;

namespace Orbito.Application.Clients.Queries.GetClientsByProvider
{
    public record GetClientsByProviderQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false,
        string? SearchTerm = null
    ) : IRequest<GetClientsByProviderResult>;

    public record GetClientsByProviderResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IEnumerable<ClientDto> Clients { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }

        public static GetClientsByProviderResult SuccessResult(
            IEnumerable<ClientDto> clients,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new GetClientsByProviderResult
            {
                Success = true,
                Clients = clients,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public static GetClientsByProviderResult FailureResult(string message)
        {
            return new GetClientsByProviderResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
