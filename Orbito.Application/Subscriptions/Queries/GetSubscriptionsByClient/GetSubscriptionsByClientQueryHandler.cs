using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient
{
    public class GetSubscriptionsByClientQueryHandler : IRequestHandler<GetSubscriptionsByClientQuery, GetSubscriptionsByClientResult>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<GetSubscriptionsByClientQueryHandler> _logger;

        public GetSubscriptionsByClientQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<GetSubscriptionsByClientQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<GetSubscriptionsByClientResult> Handle(GetSubscriptionsByClientQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subscriptions for client {ClientId} with pagination {PageNumber}/{PageSize}", 
                request.ClientId, request.PageNumber, request.PageSize);

            var subscriptions = await _subscriptionRepository.GetByClientIdAsync(request.ClientId, cancellationToken);

            if (request.ActiveOnly)
            {
                subscriptions = subscriptions.Where(s => s.Status == Domain.Enums.SubscriptionStatus.Active);
            }

            var totalCount = subscriptions.Count();
            var pagedSubscriptions = subscriptions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var subscriptionDtos = pagedSubscriptions.Select(s => new SubscriptionDto
            {
                Id = s.Id,
                ClientId = s.ClientId,
                PlanId = s.PlanId,
                Status = s.Status,
                CurrentPrice = s.CurrentPrice.Amount,
                Currency = s.CurrentPrice.Currency,
                BillingPeriod = s.BillingPeriod.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                IsInTrial = s.IsInTrial,
                TrialEndDate = s.TrialEndDate,
                CreatedAt = s.CreatedAt,
                CancelledAt = s.CancelledAt,
                PlanName = s.Plan?.Name,
                PlanDescription = s.Plan?.Description
            }).ToList();

            var result = new GetSubscriptionsByClientResult
            {
                Subscriptions = subscriptionDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            _logger.LogInformation("Successfully retrieved {Count} subscriptions for client {ClientId}", 
                subscriptionDtos.Count, request.ClientId);

            return result;
        }
    }
}
