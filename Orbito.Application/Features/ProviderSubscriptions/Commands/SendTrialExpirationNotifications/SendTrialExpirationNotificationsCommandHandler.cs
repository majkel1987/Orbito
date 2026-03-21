using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;
using Orbito.Domain.Enums;

namespace Orbito.Application.Features.ProviderSubscriptions.Commands.SendTrialExpirationNotifications;

/// <summary>
/// Handler for sending trial expiration notification emails.
/// Processes all trial subscriptions and sends appropriate notifications based on days remaining.
/// Uses TrialNotificationTier for deduplication - each tier is sent only once.
/// </summary>
public class SendTrialExpirationNotificationsCommandHandler
    : IRequestHandler<SendTrialExpirationNotificationsCommand, Result<int>>
{
    private readonly IProviderSubscriptionRepository _subscriptionRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendTrialExpirationNotificationsCommandHandler> _logger;

    public SendTrialExpirationNotificationsCommandHandler(
        IProviderSubscriptionRepository subscriptionRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<SendTrialExpirationNotificationsCommandHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        SendTrialExpirationNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting trial expiration notification check...");

        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:3000";
        var billingLink = $"{frontendBaseUrl}/dashboard/billing";

        // Get all trial subscriptions
        var trialSubscriptions = await _subscriptionRepository.GetByStatusAsync(
            ProviderSubscriptionStatus.Trial,
            cancellationToken);

        var sentCount = 0;

        foreach (var subscription in trialSubscriptions)
        {
            var daysRemaining = subscription.DaysRemaining;
            var requiredTier = GetRequiredNotificationTier(daysRemaining);

            // Skip if no notification needed or already sent
            if (requiredTier == TrialNotificationTier.None ||
                requiredTier <= subscription.LastNotificationTier)
            {
                continue;
            }

            // Get provider email
            var providerEmail = subscription.Provider?.User?.Email;
            if (string.IsNullOrEmpty(providerEmail))
            {
                _logger.LogWarning(
                    "Cannot send notification for subscription {SubscriptionId}: Provider email not found",
                    subscription.Id);
                continue;
            }

            var providerName = subscription.Provider?.BusinessName ?? "Provider";
            var planName = subscription.PlatformPlan?.Name ?? "Trial";

            // Send email
            var emailResult = await _emailService.SendTrialExpiringAsync(
                providerEmail,
                providerName,
                daysRemaining,
                planName,
                billingLink,
                cancellationToken);

            if (emailResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to send trial expiration email for subscription {SubscriptionId}: {Error}",
                    subscription.Id,
                    emailResult.Error);
                continue;
            }

            // Mark notification as sent
            var markResult = subscription.MarkNotificationSent(requiredTier);
            if (markResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to mark notification as sent for subscription {SubscriptionId}: {Error}",
                    subscription.Id,
                    markResult.Error);
                continue;
            }

            await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
            sentCount++;

            _logger.LogInformation(
                "Sent {Tier} notification to {Email} for subscription {SubscriptionId}, {DaysRemaining} days remaining",
                requiredTier,
                providerEmail,
                subscription.Id,
                daysRemaining);
        }

        // Save all changes
        if (sentCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Trial expiration notification check completed. Sent {SentCount} notifications.",
            sentCount);

        return Result.Success(sentCount);
    }

    private static TrialNotificationTier GetRequiredNotificationTier(int daysRemaining)
    {
        return daysRemaining switch
        {
            <= 1 => TrialNotificationTier.OneDay,
            <= 3 => TrialNotificationTier.ThreeDays,
            <= 5 => TrialNotificationTier.FiveDays,
            _ => TrialNotificationTier.None
        };
    }
}
