using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.ProviderSubscriptions.Queries.GetMyProviderSubscription;

/// <summary>
/// Query to get the current provider's platform subscription (trial/active status).
/// Used for displaying trial banner in the dashboard.
/// </summary>
public record GetMyProviderSubscriptionQuery : IRequest<Result<ProviderSubscriptionDto>>;

/// <summary>
/// DTO representing the provider's platform subscription status.
/// </summary>
public record ProviderSubscriptionDto
{
    public Guid Id { get; init; }
    public Guid? PlatformPlanId { get; init; }
    public string Status { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public decimal PlanPrice { get; init; }
    public string PlanCurrency { get; init; } = null!;
    public int DaysRemaining { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime? PaidUntil { get; init; }
    public bool IsTrialActive { get; init; }
    public bool IsExpired { get; init; }
}
