namespace Orbito.Application.DTOs
{
    public record SubscriptionDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid ClientId { get; init; }
        public Guid PlanId { get; init; }
        public string Status { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public int BillingPeriodValue { get; init; }
        public string BillingPeriodType { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public DateTime NextBillingDate { get; init; }
        public bool IsInTrial { get; init; }
        public DateTime? TrialEndDate { get; init; }
        public string? ExternalSubscriptionId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? CancelledAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        // Client details (optional - when IncludeDetails is true)
        public string? ClientCompanyName { get; init; }
        public string? ClientEmail { get; init; }
        public string? ClientFirstName { get; init; }
        public string? ClientLastName { get; init; }

        // Plan details (optional - when IncludeDetails is true)
        public string? PlanName { get; init; }
        public string? PlanDescription { get; init; }

        // Payment details (optional - when IncludeDetails is true)
        public int PaymentCount { get; init; }
        public decimal TotalPaid { get; init; }
        public DateTime? LastPaymentDate { get; init; }
    }
}
