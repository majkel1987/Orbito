using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;
using Orbito.Domain.Errors;

namespace Orbito.Application.Portal.Queries.GetMySubscriptions
{
    public class GetMySubscriptionsQueryHandler : IRequestHandler<GetMySubscriptionsQuery, Result<List<SubscriptionDto>>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<GetMySubscriptionsQueryHandler> _logger;

        public GetMySubscriptionsQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            IUserContextService userContextService,
            ILogger<GetMySubscriptionsQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<Result<List<SubscriptionDto>>> Handle(
            GetMySubscriptionsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subscriptions for current client via Portal");

            var clientId = await _userContextService.GetCurrentClientIdAsync(cancellationToken);
            if (clientId == null)
            {
                _logger.LogWarning("Portal: cannot resolve client ID for authenticated user");
                return Result.Failure<List<SubscriptionDto>>(DomainErrors.Client.NotFound);
            }

            var subscriptions = await _subscriptionRepository.GetByClientIdAsync(clientId.Value, cancellationToken);

            var dtos = subscriptions.Select(s => new SubscriptionDto
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

            _logger.LogInformation("Portal: returned {Count} subscriptions for client {ClientId}", dtos.Count, clientId);

            return Result.Success(dtos);
        }
    }
}
