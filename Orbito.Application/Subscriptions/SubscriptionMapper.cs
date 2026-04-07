using Orbito.Application.DTOs;
using Orbito.Domain.Entities;

namespace Orbito.Application.Subscriptions;

/// <summary>
/// Centralized mapper for Subscription entity to DTO conversions.
/// Eliminates code duplication across handlers.
/// </summary>
public static class SubscriptionMapper
{
    /// <summary>
    /// Maps a Subscription entity to SubscriptionDto.
    /// </summary>
    /// <param name="subscription">The subscription entity to map.</param>
    /// <returns>A new SubscriptionDto instance.</returns>
    public static SubscriptionDto ToDto(Subscription subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        return new SubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId.Value,
            ClientId = subscription.ClientId,
            PlanId = subscription.PlanId,
            Status = subscription.Status.ToString(),
            Amount = subscription.CurrentPrice.Amount,
            Currency = subscription.CurrentPrice.Currency,
            BillingPeriodValue = subscription.BillingPeriod.Value,
            BillingPeriodType = subscription.BillingPeriod.Type.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            NextBillingDate = subscription.NextBillingDate,
            IsInTrial = subscription.IsInTrial,
            TrialEndDate = subscription.TrialEndDate,
            ExternalSubscriptionId = subscription.ExternalSubscriptionId,
            CreatedAt = subscription.CreatedAt,
            CancelledAt = subscription.CancelledAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a Subscription entity to SubscriptionDto with additional details.
    /// WARNING: Accesses navigation properties (Client, Plan).
    /// Ensure eager-loading using .Include(s => s.Client).Include(s => s.Plan) to avoid N+1 queries.
    /// </summary>
    /// <param name="subscription">The subscription entity to map.</param>
    /// <param name="paymentCount">Total number of payments.</param>
    /// <param name="totalPaid">Total amount paid.</param>
    /// <param name="lastPaymentDate">Date of last payment.</param>
    /// <returns>A new SubscriptionDto instance with details.</returns>
    public static SubscriptionDto ToDtoWithDetails(
        Subscription subscription,
        int paymentCount = 0,
        decimal totalPaid = 0,
        DateTime? lastPaymentDate = null)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        var dto = ToDto(subscription);

        return dto with
        {
            ClientCompanyName = subscription.Client?.CompanyName,
            ClientEmail = subscription.Client?.Email?.ToString(),
            ClientFirstName = subscription.Client?.FirstName,
            ClientLastName = subscription.Client?.LastName,
            PlanName = subscription.Plan?.Name,
            PlanDescription = subscription.Plan?.Description,
            PaymentCount = paymentCount,
            TotalPaid = totalPaid,
            LastPaymentDate = lastPaymentDate
        };
    }
}
