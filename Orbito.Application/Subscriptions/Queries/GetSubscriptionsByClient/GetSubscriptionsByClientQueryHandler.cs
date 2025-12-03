using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionsByClient
{
    public class GetSubscriptionsByClientQueryHandler : IRequestHandler<GetSubscriptionsByClientQuery, Result<Common.Models.PaginatedList<SubscriptionDto>>>
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

        public async Task<Result<Common.Models.PaginatedList<SubscriptionDto>>> Handle(GetSubscriptionsByClientQuery request, CancellationToken cancellationToken)
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
                TenantId = s.TenantId.Value,
                ClientId = s.ClientId,
                PlanId = s.PlanId,
                Status = s.Status.ToString(),
                Amount = s.CurrentPrice.Amount,
                Currency = s.CurrentPrice.Currency,
                BillingPeriodValue = s.BillingPeriod.Value,
                BillingPeriodType = s.BillingPeriod.Type.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                NextBillingDate = s.NextBillingDate,
                IsInTrial = s.IsInTrial,
                TrialEndDate = s.TrialEndDate,
                ExternalSubscriptionId = s.ExternalSubscriptionId,
                CreatedAt = s.CreatedAt,
                CancelledAt = s.CancelledAt,
                UpdatedAt = s.UpdatedAt,
                PlanName = s.Plan?.Name,
                PlanDescription = s.Plan?.Description
            }).ToList();

            var paginatedList = new Common.Models.PaginatedList<SubscriptionDto>(
                subscriptionDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} subscriptions for client {ClientId}",
                subscriptionDtos.Count, request.ClientId);

            return Result.Success(paginatedList);
        }
    }
}
