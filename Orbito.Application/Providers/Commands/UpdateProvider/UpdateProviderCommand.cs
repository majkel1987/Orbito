using MediatR;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;

namespace Orbito.Application.Providers.Commands.UpdateProvider
{
    public record UpdateProviderCommand(
        Guid Id,
        string BusinessName,
        string? Description = null,
        string? Avatar = null,
        string? SubdomainSlug = null,
        string? CustomDomain = null
    ) : IRequest<UpdateProviderResult>;

    public record UpdateProviderResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public ProviderDto? Provider { get; init; }
        public List<string> Errors { get; init; } = new();

        public static UpdateProviderResult SuccessResult(ProviderDto provider)
        {
            return new UpdateProviderResult
            {
                Success = true,
                Message = "Provider updated successfully",
                Provider = provider
            };
        }

        public static UpdateProviderResult FailureResult(string message, List<string>? errors = null)
        {
            return new UpdateProviderResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
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
