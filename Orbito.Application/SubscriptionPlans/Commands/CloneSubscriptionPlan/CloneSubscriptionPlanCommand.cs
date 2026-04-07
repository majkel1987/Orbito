using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.SubscriptionPlans.Commands.CloneSubscriptionPlan;

public record CloneSubscriptionPlanCommand : IRequest<Result<CloneSubscriptionPlanResult>>
{
    public Guid Id { get; init; }
    public string NewName { get; init; } = string.Empty;
    public string? NewDescription { get; init; }
    public decimal? NewAmount { get; init; }
    public string? NewCurrency { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPublic { get; init; } = true;
    public int? NewSortOrder { get; init; }
}
