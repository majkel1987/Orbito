using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Application.Subscriptions.Queries.GetSubscriptionById
{
    public class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, GetSubscriptionByIdResult?>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ILogger<GetSubscriptionByIdQueryHandler> _logger;

        public GetSubscriptionByIdQueryHandler(
            ISubscriptionRepository subscriptionRepository,
            ILogger<GetSubscriptionByIdQueryHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<GetSubscriptionByIdResult?> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting subscription {SubscriptionId} with details: {IncludeDetails}", 
                request.SubscriptionId, request.IncludeDetails);

            var subscription = request.IncludeDetails
                ? await _subscriptionRepository.GetByIdWithDetailsAsync(request.SubscriptionId, cancellationToken)
                : await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", request.SubscriptionId);
                return null;
            }

            var result = new GetSubscriptionByIdResult
            {
                Id = subscription.Id,
                ClientId = subscription.ClientId,
                PlanId = subscription.PlanId,
                Status = subscription.Status,
                CurrentPrice = subscription.CurrentPrice.Amount,
                Currency = subscription.CurrentPrice.Currency,
                BillingPeriod = subscription.BillingPeriod.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextBillingDate = subscription.NextBillingDate,
                IsInTrial = subscription.IsInTrial,
                TrialEndDate = subscription.TrialEndDate,
                CreatedAt = subscription.CreatedAt,
                CancelledAt = subscription.CancelledAt,
                UpdatedAt = subscription.UpdatedAt
            };

            if (request.IncludeDetails)
            {
                result = result with
                {
                    ClientCompanyName = subscription.Client?.CompanyName,
                    ClientEmail = subscription.Client?.DirectEmail,
                    ClientFirstName = subscription.Client?.DirectFirstName,
                    ClientLastName = subscription.Client?.DirectLastName,
                    PlanName = subscription.Plan?.Name,
                    PlanDescription = subscription.Plan?.Description,
                    PaymentCount = subscription.Payments?.Count ?? 0,
                    TotalPaid = subscription.Payments?.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Sum(p => p.Amount.Amount) ?? 0,
                    LastPaymentDate = subscription.Payments?.Where(p => p.Status == Domain.Enums.PaymentStatus.Completed).Max(p => p.ProcessedAt)
                };
            }

            _logger.LogInformation("Successfully retrieved subscription {SubscriptionId}", request.SubscriptionId);

            return result;
        }
    }
}
