using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Features.Analytics.Queries.GetDashboardStats;

/// <summary>
/// Handler for get dashboard statistics query
/// </summary>
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IClientRepository _clientRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;

    public GetDashboardStatsQueryHandler(
        IClientRepository clientRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentRepository paymentRepository,
        ITenantContext tenantContext,
        ILogger<GetDashboardStatsQueryHandler> logger)
    {
        _clientRepository = clientRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentRepository = paymentRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting dashboard stats for period {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            var tenantId = _tenantContext.CurrentTenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("No tenant context available for dashboard stats");
                return new DashboardStatsDto
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
            }

            // Get client statistics
            var totalClients = await _clientRepository.GetActiveClientsCountAsync(cancellationToken: cancellationToken);

            // Get subscription statistics
            var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsCountAsync(cancellationToken);
            var totalRevenue = await _subscriptionRepository.GetTotalRevenueAsync(cancellationToken);

            // Get active subscriptions to calculate MRR
            var activeSubscriptionsList = await _subscriptionRepository.GetActiveSubscriptionsForTenantAsync(tenantId, cancellationToken);

            decimal mrr = 0;
            int cancelledCount = 0;

            foreach (var sub in activeSubscriptionsList)
            {
                // Calculate monthly equivalent based on billing period type
                var monthlyAmount = sub.BillingPeriod.Type switch
                {
                    BillingPeriodType.Monthly => sub.CurrentPrice.Amount,
                    BillingPeriodType.Yearly => sub.CurrentPrice.Amount / 12,
                    BillingPeriodType.Weekly => sub.CurrentPrice.Amount * 4, // ~4 weeks per month
                    BillingPeriodType.Daily => sub.CurrentPrice.Amount * 30, // ~30 days per month
                    _ => sub.CurrentPrice.Amount
                };
                mrr += monthlyAmount;

                // Count cancelled in period
                if (sub.Status == SubscriptionStatus.Cancelled &&
                    sub.CancelledAt >= request.StartDate &&
                    sub.CancelledAt <= request.EndDate)
                {
                    cancelledCount++;
                }
            }

            // Calculate ARR (Annual Recurring Revenue)
            var arr = mrr * 12;

            // Calculate churn rate: (cancelled subscriptions / total at start) * 100
            var totalAtStart = activeSubscriptions + cancelledCount;
            var churnRate = totalAtStart > 0 ? (decimal)cancelledCount / totalAtStart * 100 : 0;

            var result = new DashboardStatsDto
            {
                Mrr = Math.Round(mrr, 2),
                Arr = Math.Round(arr, 2),
                TotalClients = totalClients,
                ActiveSubscriptions = activeSubscriptions,
                ChurnRate = Math.Round(churnRate, 2),
                TotalRevenue = Math.Round(totalRevenue, 2),
                Currency = "PLN",
                NewClients = 0, // Would require CreatedAt filtering on clients
                CancelledSubscriptions = cancelledCount,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            _logger.LogInformation("Dashboard stats calculated: MRR={Mrr}, ARR={Arr}, Clients={Clients}, ActiveSubs={ActiveSubs}",
                result.Mrr, result.Arr, result.TotalClients, result.ActiveSubscriptions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats for period {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            return new DashboardStatsDto
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
    }
}
