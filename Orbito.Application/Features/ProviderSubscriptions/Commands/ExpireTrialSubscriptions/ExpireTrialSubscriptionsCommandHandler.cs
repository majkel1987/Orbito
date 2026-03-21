using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.ExpireTrialSubscriptions;

/// <summary>
/// Handler for expiring trial subscriptions.
/// Processes all trial subscriptions that have passed their trial end date,
/// changes their status to Expired, and sends notification emails.
/// </summary>
public class ExpireTrialSubscriptionsCommandHandler
    : IRequestHandler<ExpireTrialSubscriptionsCommand, Result<int>>
{
    private readonly IProviderSubscriptionRepository _subscriptionRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpireTrialSubscriptionsCommandHandler> _logger;

    public ExpireTrialSubscriptionsCommandHandler(
        IProviderSubscriptionRepository subscriptionRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<ExpireTrialSubscriptionsCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        ExpireTrialSubscriptionsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting trial expiration check...");

        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var billingLink = $"{frontendBaseUrl}/dashboard/billing";

        // Get all expired trial subscriptions (Status == Trial AND TrialEndDate < now)
        var expiredTrials = await _subscriptionRepository.GetExpiredTrialsAsync(cancellationToken);

        var expiredCount = 0;

        foreach (var subscription in expiredTrials)
        {
            // Change status to Expired
            var expireResult = subscription.Expire();
            if (expireResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to expire subscription {SubscriptionId}: {Error}",
                    subscription.Id,
                    expireResult.Error);
                continue;
            }

            // Get provider email
            var providerEmail = subscription.Provider?.User?.Email;
            if (string.IsNullOrEmpty(providerEmail))
            {
                _logger.LogWarning(
                    "Cannot send expiration email for subscription {SubscriptionId}: Provider email not found",
                    subscription.Id);

                // Still update the subscription status
                await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
                expiredCount++;
                continue;
            }

            var providerName = subscription.Provider?.BusinessName ?? "Provider";
            var planName = subscription.PlatformPlan?.Name ?? "Plan";

            // Send expiration email with payment instructions
            var emailResult = await _emailService.SendTrialExpiredAsync(
                providerEmail,
                providerName,
                planName,
                billingLink,
                cancellationToken);

            if (emailResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to send trial expiration email for subscription {SubscriptionId}: {Error}",
                    subscription.Id,
                    emailResult.Error);
            }
            else
            {
                _logger.LogInformation(
                    "Sent trial expiration email to {Email} for subscription {SubscriptionId}",
                    providerEmail,
                    subscription.Id);
            }

            // Mark notification as sent (Expired tier)
            subscription.MarkNotificationSent(TrialNotificationTier.Expired);

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            expiredCount++;

            _logger.LogInformation(
                "Expired subscription {SubscriptionId} for provider {ProviderId}",
                subscription.Id,
                subscription.ProviderId);
        }

        // Save all changes
        if (expiredCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Trial expiration check completed. Expired {ExpiredCount} subscriptions.",
            expiredCount);

        return Result.Success(expiredCount);
    }
}
