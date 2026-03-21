using Orbito.Domain.Common;

namespace Orbito.Application.Common.Interfaces;

public interface IEmailService
{
    Task<Result> SendClientInvitationAsync(
        string toEmail,
        string clientName,
        string providerName,
        string invitationLink,
        CancellationToken cancellationToken = default);

    Task<Result> SendPaymentConfirmationAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string paymentId,
        DateTime paymentDate,
        CancellationToken cancellationToken = default);

    Task<Result> SendPaymentFailedAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string failureReason,
        CancellationToken cancellationToken = default);

    Task<Result> SendTrialExpiringAsync(
        string toEmail,
        string providerName,
        int daysRemaining,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default);

    Task<Result> SendTrialExpiredAsync(
        string toEmail,
        string providerName,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default);
}
