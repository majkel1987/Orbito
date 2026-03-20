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
}
