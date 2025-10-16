using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Queries.GetActiveSubscriptions
{
    public class GetActiveSubscriptionsQueryHandler : IRequestHandler<GetActiveSubscriptionsQuery, GetActiveSubscriptionsResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<GetActiveSubscriptionsQueryHandler> _logger;

        public GetActiveSubscriptionsQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<GetActiveSubscriptionsQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<GetActiveSubscriptionsResult> Handle(GetActiveSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting active subscriptions with pagination {PageNumber}/{PageSize}", 
                request.PageNumber, request.PageSize);

            // NOTE: Using deprecated method because this query is only accessible by Providers and PlatformAdmins
            // who have proper authorization to view all subscriptions in their tenant
#pragma warning disable CS0618 // Type or member is obsolete
            var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                subscriptions = subscriptions.Where(s => 
                    s.Client?.CompanyName?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Client?.DirectEmail?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    s.Plan?.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }

            var totalCount = subscriptions.Count();
            var pagedSubscriptions = subscriptions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var subscriptionDtos = pagedSubscriptions.Select(s => new ActiveSubscriptionDto
            {
                Id = s.Id,
                ClientId = s.ClientId,
                PlanId = s.PlanId,
                Status = s.Status,
                CurrentPrice = s.CurrentPrice.Amount,
                Currency = s.CurrentPrice.Currency,
                BillingPeriod = s.BillingPeriod.ToString(),
                StartDate = s.StartDate,
                NextBillingDate = s.NextBillingDate,
                IsInTrial = s.IsInTrial,
                TrialEndDate = s.TrialEndDate,
                CreatedAt = s.CreatedAt,
                ClientCompanyName = s.Client?.CompanyName,
                ClientEmail = s.Client?.DirectEmail,
                PlanName = s.Plan?.Name,
                PlanDescription = s.Plan?.Description
            }).ToList();

            var result = new GetActiveSubscriptionsResult
            {
                Subscriptions = subscriptionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            _logger.LogInformation("Successfully retrieved {Count} active subscriptions", subscriptionDtos.Count);

            return result;
        }
    }
}
