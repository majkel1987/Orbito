using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Queries.GetAllProviders
{
    public record GetAllProvidersQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool ActiveOnly = false
    ) : IRequest<GetAllProvidersResult>;

    public record GetAllProvidersResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IEnumerable<ProviderSummaryDto> Providers { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }

        public static GetAllProvidersResult SuccessResult(
            IEnumerable<ProviderSummaryDto> providers,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new GetAllProvidersResult
            {
                Success = true,
                Providers = providers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public static GetAllProvidersResult FailureResult(string message)
        {
            return new GetAllProvidersResult
            {
                Success = false,
                Message = message
            };
        }
    }

    public record ProviderSummaryDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public string BusinessName { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Avatar { get; init; }
        public string SubdomainSlug { get; init; } = string.Empty;
        public string? CustomDomain { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public decimal MonthlyRevenue { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int ActiveClientsCount { get; init; }
        public int PlansCount { get; init; }
        public int SubscriptionsCount { get; init; }
        public string? UserEmail { get; init; }
        public string? UserFullName { get; init; }
    }
}
