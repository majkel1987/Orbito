using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Queries.GetExpiringSubscriptions
{
    public class GetExpiringSubscriptionsQueryHandler : IRequestHandler<GetExpiringSubscriptionsQuery, GetExpiringSubscriptionsResult>
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<GetExpiringSubscriptionsQueryHandler> _logger;

        public GetExpiringSubscriptionsQueryHandler(
            ISubscriptionService subscriptionService,
            ILogger<GetExpiringSubscriptionsQueryHandler> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public async Task<GetExpiringSubscriptionsResult> Handle(GetExpiringSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting expiring subscriptions within {Days} days", request.DaysBeforeExpiration);

            var expiringSubscriptions = await _subscriptionService.GetExpiringSubscriptionsAsync(
                request.DaysBeforeExpiration, cancellationToken);

            var totalCount = expiringSubscriptions.Count();
            var pagedSubscriptions = expiringSubscriptions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var subscriptionDtos = pagedSubscriptions.Select(s => new ExpiringSubscriptionDto
            {
                Id = s.Id,
                ClientId = s.ClientId,
                PlanId = s.PlanId,
                Status = s.Status,
                CurrentPrice = s.CurrentPrice.Amount,
                Currency = s.CurrentPrice.Currency,
                NextBillingDate = s.NextBillingDate,
                IsInTrial = s.IsInTrial,
                TrialEndDate = s.TrialEndDate,
                ClientCompanyName = s.Client?.CompanyName,
                ClientEmail = s.Client?.DirectEmail,
                PlanName = s.Plan?.Name,
                DaysUntilExpiration = (int)(s.NextBillingDate - DateTime.UtcNow).TotalDays
            }).ToList();

            var result = new GetExpiringSubscriptionsResult
            {
                Subscriptions = subscriptionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                DaysBeforeExpiration = request.DaysBeforeExpiration
            };

            _logger.LogInformation("Successfully retrieved {Count} expiring subscriptions", subscriptionDtos.Count);

            return result;
        }
    }
}
