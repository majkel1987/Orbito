namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider
{
    public record SubscriptionPlansListDto
    {
        public List<SubscriptionPlanListItemDto> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public bool HasPreviousPage { get; init; }
        public bool HasNextPage { get; init; }
    }

    public record SubscriptionPlanListItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string BillingPeriod { get; init; } = string.Empty;
        public int TrialPeriodDays { get; init; }
        public bool IsActive { get; init; }
        public bool IsPublic { get; init; }
        public int SortOrder { get; init; }
        public DateTime CreatedAt { get; init; }
        public int ActiveSubscriptionsCount { get; init; }
        public int TotalSubscriptionsCount { get; init; }
    }
}
