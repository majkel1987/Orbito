using MediatR;

namespace Orbito.Application.Clients.Queries.GetClientStats
{
    public record GetClientStatsQuery() : IRequest<GetClientStatsResult>;

    public record GetClientStatsResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ClientStatsDto? Stats { get; init; }

        public static GetClientStatsResult SuccessResult(ClientStatsDto stats)
        {
            return new GetClientStatsResult
            {
                Success = true,
                Stats = stats
            };
        }

        public static GetClientStatsResult FailureResult(string message)
        {
            return new GetClientStatsResult
            {
                Success = false,
                Message = message
            };
        }
    }

    public record ClientStatsDto
    {
        public int TotalClients { get; init; }
        public int ActiveClients { get; init; }
        public int InactiveClients { get; init; }
        public int ClientsWithIdentity { get; init; }
        public int DirectClients { get; init; }
        public int ClientsWithActiveSubscriptions { get; init; }
        public decimal TotalRevenue { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime LastUpdated { get; init; }
    }
}
