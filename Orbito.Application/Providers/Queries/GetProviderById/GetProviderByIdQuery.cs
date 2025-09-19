using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Queries.GetProviderById
{
    public record GetProviderByIdQuery(Guid Id) : IRequest<GetProviderByIdResult>;

    public record GetProviderByIdResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ProviderDto? Provider { get; init; }

        public static GetProviderByIdResult SuccessResult(ProviderDto provider)
        {
            return new GetProviderByIdResult
            {
                Success = true,
                Provider = provider
            };
        }

        public static GetProviderByIdResult NotFoundResult(string message = "Provider not found")
        {
            return new GetProviderByIdResult
            {
                Success = false,
                Message = message
            };
        }
    }

    public record ProviderDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid? UserId { get; init; }
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
        public string? UserFirstName { get; init; }
        public string? UserLastName { get; init; }
    }
}
