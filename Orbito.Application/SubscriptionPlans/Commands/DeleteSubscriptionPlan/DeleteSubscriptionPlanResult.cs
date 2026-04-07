namespace Orbito.Application.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public record DeleteSubscriptionPlanResult
{
    public Guid Id { get; init; }
    public bool IsDeleted { get; init; }
    public bool IsHardDelete { get; init; }
    public string Message { get; init; } = string.Empty;
}
