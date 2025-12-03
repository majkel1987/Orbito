namespace Orbito.Application.DTOs
{
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

